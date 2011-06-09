using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ManifoldMapEdit
{
    public enum MODETYPE_OPERATION
    {
        /// <summary>
        /// Default functionality.
        /// </summary>
        SelectMode,

        /// <summary>
        /// Draw functionality.
        /// </summary>
        DrawPoint,
        DrawLine,
        DrawArea,

        /// <summary>
        /// Edit Functionality.
        /// </summary>
        EditModeEnabled, //Move coordinate is enabled in this mode
        EditAddCoordinate,
        EditDeleteCoordinate,
        EditMoveGeom,

        /// <summary>
        /// Snap Functionality.
        /// </summary>
        SnapModeEnabled,
        SnapModeStarted,
        SnapToFrom
    }

    public struct GeomMovedDetails
    {
        public double StartX;
        public double EndX;
        public double MoveHorizontallyX;

        public double StartY;
        public double EndY;
        public double MoveVerticallyY;
    }

    /// <summary>
    /// This class is used to replicate the Edit Tool bar functionality of the Manifold application.
    /// It currently deals with:
    ///   1. Move Coordinate (default mode)
    ///   2. Add Coordinate
    ///   3. Delete Coordinate
    ///   4. Move Entire Geom
    /// It is an internal class that and is used within the ManifoldInspections.InspectionDetails.ManifoldInspection class
    /// to provide Edit finctionality for the inspections that inherit from this class.
    /// </summary>
    public class EditDrawingFactorySQL
    {
        public StringBuilder sbOUTPUT = new StringBuilder();
        #region Variables
        //private Drawing.DrawManifold _DrawManifold;
        // Track whether Dispose has been called.
        private bool _DISPOSED = false;

        private Int32 _SelectedManifoldID;
        /// <summary>
        /// This is the original GEOM string before it was edited via the 'EditDrawingFactorySQL' class.
        /// This needs set from the 'EditDrawingFactorySQL' class when a GEOM is edited because if editing
        /// an Exclusion GEOM then need the original GEOM to recalculate the Parent containing GEOM.
        /// </summary>
        private string _SelectedManifoldObjectOriginalGEOM;
        private Int32 _SelectedEditManifoldID;
        private Int32 _SelectedPointIndex = -1;
        private Int32 _SelectedPointBranchIndex = -1;

        /// <summary>
        /// When Mode is: MoveGeom the _SelectedEditManifoldPointStart and _SelectedEditManifoldPointEnd Points are the Points to take the
        /// difference in X and Y coordinates in meters in order to Move the selected Point.
        /// </summary>
        private Manifold.Interop.Point _SelectedEditManifoldPointStart;
        private Manifold.Interop.Point _SelectedEditManifoldPointEnd;
        private Manifold.Interop.BranchSet _OriginalObjectBranchSet;
        private Manifold.Interop.Drawing _DrawingOriginalEditObject;
        private Manifold.Interop.Drawing _DrawingEdit;
        private AxManifold.Interop.AxComponentControl _MapControl;
        private bool _MouseIsClickedDown = false;
        private MODETYPE_OPERATION _EditModeOperation;
        private bool _UserMovedCoordinate = false;
        private bool _UserAddedCoordinate = false;
        private bool _UserDeletedCoordinate = false;
        private bool _UserMovedGeom = false;

        private GeomMovedDetails _GeomMovedDetails = new GeomMovedDetails();

        private double _SelectionTolerance = 40;
        private double _YCoordMapClickOffset = 27;//Bug in Manifold MapControl means Y Coord is inaccurate if the statusbar is visible.
        private double _SelectionToleranceAddCoordinate = 0.05;
        private string _Edit_ThemePoint = "Edit Point";
        private bool _EditPointsPopulated = false;

        //When processing Add Coordinate this should be set to True - subsequent clicks should be ignored until
        // processing on first click is completed.
        private bool _AddingCoordinate = false;

        public Manifold.Interop.Drawing DrawingOriginalEditObject
        {
            get
            {
                return _DrawingOriginalEditObject;
            }
            set
            {
                _DrawingOriginalEditObject = value;
            }
        }

        public Manifold.Interop.Drawing DrawingEdit
        {
            get
            {
                return _DrawingEdit;
            }
            set
            {
                _DrawingEdit = value;
            }
        }

        public double YCoordMapClickOffset
        {
            get
            {
                return _YCoordMapClickOffset;
            }
        }

        public double SelectionTolerance
        {
            get
            {
                return _SelectionTolerance;
            }
            set
            {
                _SelectionTolerance = value;
            }
        }

        public double SelectionToleranceAddCoordinate
        {
            get
            {
                return _SelectionToleranceAddCoordinate;
            }
            set
            {
                _SelectionToleranceAddCoordinate = value;
            }
        }

        #endregion

        #region Events

        #region MODETYPE_OPERATION Changed
        private bool inEventModeTypeOperationChanged = false;

        /// <summary>
        /// Get\Set method: On the set method the OnMapModeTypeChanged method is called which calls the 
        /// MapModeTypeChanged event to alert subscribers\listeners of this event that the mode has changed.
        /// </summary>
        public MODETYPE_OPERATION EditModeOperation
        {
            get
            {
                return _EditModeOperation;
            }
            set
            {
                if (this._EditModeOperation != value)
                {
                    this._EditModeOperation = value;
                    switch (value)
                    {
                        case MODETYPE_OPERATION.EditModeEnabled:
                            if (validateEditObjects() == true)
                            {
                                prepareEditObject();
                            }
                            break;
                        case MODETYPE_OPERATION.EditAddCoordinate:
                            prepareEditObject();
                            break;
                        case MODETYPE_OPERATION.EditDeleteCoordinate:
                            prepareEditObject();
                            break;
                        case MODETYPE_OPERATION.EditMoveGeom:
                            prepareEditObject();
                            break;
                        default:
                            this._SelectedManifoldID = 0;
                            clearEditLayer();
                            break;
                    }

                    OnMapModeTypeChanged(new ModeTypeEventArgs(this._EditModeOperation));
                }
            }
        }

        //By using the generic EventHandler<T> event type we do not need to declare a separate delegate type.
        public event EventHandler<ModeTypeEventArgs> MapModeTypeChanged;

        /// <summary>
        /// If the ModeType for the MapControl is changed then raise this event to inform any LISTNERS of
        /// the change.
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnMapModeTypeChanged(ModeTypeEventArgs e)
        {
            try
            {
                // Make a temporary copy of the event to avoid possibility of
                // a race condition if the last subscriber unsubscribes
                // immediately after the null check and before the event is raised.
                EventHandler<ModeTypeEventArgs> handler = MapModeTypeChanged;
                if (handler != null)
                {
                    if (inEventModeTypeOperationChanged == false)
                    {
                        inEventModeTypeOperationChanged = true;
                        // Call the Event
                        handler(this, e);

                        if (this._EditModeOperation != e.MODETYPE_OPERATION)
                        {
                            this.EditModeOperation = e.MODETYPE_OPERATION;
                        }
                    }
                }
            }
            catch (Exception objEx)
            {
                throw;
            }
            finally
            {
                inEventModeTypeOperationChanged = false;
            }
        }


        #endregion

        #region GEOM Updated

        // The event. Note that by using the generic EventHandler<T> event type
        // we do not need to declare a separate delegate type.
        public event EventHandler<DrawEventArgs> GEOMChanged;

        ///// <summary>
        ///// If an GEOM (Point, Line, Area) is updated via Edit shape or Add\Remove point then need to call this
        ///// event to tell other objects that a GEOM was changed.
        /////     e.g.  If GEOM was updated which was an Exclusion then need to tell some class to recalculate the size 
        /////           of the Parent object that contains this exclusion.
        ///// </summary>
        protected virtual void OnGEOMChanged(DrawEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<DrawEventArgs> handler = GEOMChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion

        #endregion

        #region Constructor and Deconstructor

        //ManifoldInspections.InspectionDetails.CommandDrawing

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_MapControl">Reference to the Manifold ActiveX control.</param>
        /// <param name="p_DrawingEdit">A temporary Drawing layer - used only to display inflection Points of the Line\area to edit.</param>
        /// <param name="p_Draw">Internal class used to draw ojects on Drawings.</param>
        /// <param name="p_MODETYPE_OPERATION"></param>
        public EditDrawingFactorySQL(AxManifold.Interop.AxComponentControl p_MapControl,  Manifold.Interop.Drawing p_DrawingEdit, MODETYPE_OPERATION p_MODETYPE_OPERATION)
        {
            try
            {
                //Map events
                this._MapControl = p_MapControl;
                this._MapControl.MouseDownEvent += new AxManifold.Interop.IComponentControlEvents_MouseDownEventHandler(this.MapControl_MouseDownEvent);
                this._MapControl.MouseUpEvent += new AxManifold.Interop.IComponentControlEvents_MouseUpEventHandler(this.MapControl_MouseUpEvent);
                this._MapControl.MouseMoveEvent += new AxManifold.Interop.IComponentControlEvents_MouseMoveEventHandler(this.MapControl_MouseMoveEvent);

                if (p_DrawingEdit == null)
                {
                    throw new Exception("The Edit Drawing layer was null in the EditDrawingFactory constructor. Assign an initialised Manifold.Interop.Drawing to it before calling this class.");
                }
                else
                {
                    this._DrawingEdit = p_DrawingEdit;
                }
                this._EditModeOperation = p_MODETYPE_OPERATION;
                clearEditLayer();
            }
            catch (Exception objEx)
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_MapControl">Reference to the Manifold ActiveX control.</param>
        /// <param name="p_DrawingOriginalEditObject">The Drawing layer which contains the original Line\Area to edit.</param>
        /// <param name="p_DrawingEdit">A temporary Drawing layer - used only to display inflection Points of the Line\area to edit.</param>
        /// <param name="p_Draw">Internal class used to draw ojects on Drawings.</param>
        /// <param name="p_MODETYPE_OPERATION"></param>
        public EditDrawingFactorySQL(AxManifold.Interop.AxComponentControl p_MapControl, Manifold.Interop.Drawing p_DrawingOriginalEditObject, Manifold.Interop.Drawing p_DrawingEdit, MODETYPE_OPERATION p_MODETYPE_OPERATION)
        {
            try
            {
                //Map events
                this._MapControl = p_MapControl;

                this._MapControl.MouseDownEvent += new AxManifold.Interop.IComponentControlEvents_MouseDownEventHandler(this.MapControl_MouseDownEvent);
                this._MapControl.MouseUpEvent += new AxManifold.Interop.IComponentControlEvents_MouseUpEventHandler(this.MapControl_MouseUpEvent);
                this._MapControl.MouseMoveEvent += new AxManifold.Interop.IComponentControlEvents_MouseMoveEventHandler(this.MapControl_MouseMoveEvent);

                if (p_DrawingOriginalEditObject == null )
                {
                    throw new Exception("The Original Edit objects Drawing was null in the EditDrawingFactory constructor. Assign an initialised Manifold.Interop.Drawing to it before calling this class.");
                }
                else
                {
                    this._DrawingOriginalEditObject = p_DrawingOriginalEditObject;
                }

                if (p_DrawingEdit == null)
                {
                    throw new Exception("The Edit Drawing layer was null in the EditDrawingFactory constructor. Assign an initialised Manifold.Interop.Drawing to it before calling this class.");
                }
                else
                {
                    this._DrawingEdit = p_DrawingEdit;
                }
                this._EditModeOperation = p_MODETYPE_OPERATION;
                clearEditLayer();
            }
            catch (Exception objEx)
            {
                throw;
            }
        }

        ~EditDrawingFactorySQL()
        {
            try
            {
                this._MapControl.MouseDownEvent -= new AxManifold.Interop.IComponentControlEvents_MouseDownEventHandler(this.MapControl_MouseDownEvent);
                this._MapControl.MouseUpEvent -= new AxManifold.Interop.IComponentControlEvents_MouseUpEventHandler(this.MapControl_MouseUpEvent);
                this._MapControl.MouseMoveEvent -= new AxManifold.Interop.IComponentControlEvents_MouseMoveEventHandler(this.MapControl_MouseMoveEvent);
            }
            catch (Exception)
            {
            }
        }

        #endregion

        #region Map Control Events
        private void MapControl_MouseDownEvent(object sender, AxManifold.Interop.IComponentControlEvents_MouseDownEvent e)
        {
            try
            {
                if (Utility.isModeTypeEditEnabled(EditModeOperation) == true)
                {
                    this._DrawingEdit.SelectNone();
                    if (this._DrawingOriginalEditObject.Selection.Count < 1 || Convert.ToInt32(this._DrawingOriginalEditObject.Selection[0].Record.get_Data("ID").ToString()) != this._SelectedManifoldID)
                    {
                        Utility.setSelection(this._MapControl, this._DrawingOriginalEditObject, this._SelectedManifoldID);
                    }

                    if (this._DrawingEdit.Selection.Count < 1)
                    {
                        //Save the Edit object GEOM string before any EDITING is performed on it.
                        this._SelectedManifoldObjectOriginalGEOM = this._DrawingOriginalEditObject.Selection[0].get_Geom().ToTextWKT();

                        //SET SELECTION
                        Manifold.Interop.Point objPointScreen = e.pArgs.LocationScreen;
                        objPointScreen.Y += this._YCoordMapClickOffset;
                        objPointScreen = this._MapControl.ScreenToNative(objPointScreen);

                        switch (EditModeOperation)
                        {
                            case MODETYPE_OPERATION.EditDeleteCoordinate:
                                this._MouseIsClickedDown = true;
                                deleteCoordinate(objPointScreen);
                                break;
                            case MODETYPE_OPERATION.EditAddCoordinate:
                                this._MouseIsClickedDown = true;
                                if (this._AddingCoordinate == false)
                                {
                                    addCoordinate(objPointScreen);
                                }
                                break;
                            case MODETYPE_OPERATION.EditModeEnabled:
                                Utility.setNearestSelection(this._MapControl, this._DrawingEdit, objPointScreen, _SelectionTolerance);

                                if (this._DrawingEdit.Selection.Count > 0)
                                {
                                    //Record selected Point object
                                    this._MouseIsClickedDown = true;
                                    this._SelectedEditManifoldID = Convert.ToInt32(this._DrawingEdit.Selection[0].Record.get_Data("ID").ToString());

                                    this._SelectedPointBranchIndex = Convert.ToInt32(this._DrawingEdit.Selection[0].Record.get_Data("BranchIndex").ToString());
                                    this._SelectedPointIndex = Convert.ToInt32(this._DrawingEdit.Selection[0].Record.get_Data("PointIndex").ToString());
                                }
                                else
                                {
                                    this._SelectedEditManifoldID = -1;
                                }
                                break;
                            case MODETYPE_OPERATION.EditMoveGeom:
                                Utility.setNearestSelection(this._MapControl, this._DrawingEdit, objPointScreen, _SelectionTolerance);

                                if (this._DrawingEdit.Selection.Count > 0)
                                {
                                    this._GeomMovedDetails = new GeomMovedDetails();
                                    this._UserMovedGeom = false;
                                    //Record selected Point object
                                    this._MouseIsClickedDown = true;
                                    this._SelectedEditManifoldID = Convert.ToInt32(this._DrawingEdit.Selection[0].Record.get_Data("ID").ToString());
                                    this._SelectedEditManifoldPointStart = e.pArgs.LocationNative;
                                }
                                else
                                {
                                    this._SelectedEditManifoldID = -1;
                                }
                                break;
                            default:
                                this._MouseIsClickedDown = false;
                                break;
                        }
                    }
                }
            }
            catch (Exception objEx)
            {
                throw;
            }
        }

        private void MapControl_MouseUpEvent(object sender, AxManifold.Interop.IComponentControlEvents_MouseUpEvent e)
        {
            try
            {
                bool bPrepareEdit = false;

                if (this._MouseIsClickedDown == true &&
                  (Utility.isModeTypeEditEnabled(EditModeOperation) == true))
                {
                    //Need to set this here because it gets updated in the 'prepareEditObject()' methods
                    //and the GEOMChanged Event needs the original value before it was Edited.
                    //string sSelectedManifoldObjectOriginalGEOM = this._SelectedManifoldObjectOriginalGEOM;

                    switch (EditModeOperation)
                    {
                        case MODETYPE_OPERATION.EditDeleteCoordinate:
                            this._DrawingEdit.SelectNone();
                            break;
                        case MODETYPE_OPERATION.EditAddCoordinate:
                            this._DrawingEdit.SelectNone();
                            break;
                        case MODETYPE_OPERATION.EditModeEnabled:
                            if (this._SelectedEditManifoldID > 0 && this._UserMovedCoordinate == true)
                            {
                                Manifold.Interop.Point objPointScreen = e.pArgs.LocationScreen;
                                objPointScreen.Y += this._YCoordMapClickOffset;
                                objPointScreen = this._MapControl.ScreenToNative(objPointScreen);
                                moveOriginalPointCoordinate(objPointScreen.X, objPointScreen.Y);
                                bPrepareEdit = true;

                                //After MouseUp Event - remove selection for move so that it dosnt move anymore
                                this._SelectedEditManifoldID = -1;
                            }
                            break;
                        case MODETYPE_OPERATION.EditMoveGeom:
                            if (this._SelectedEditManifoldID > 0 && this._UserMovedGeom == true)
                            {
                                this._SelectedEditManifoldPointEnd = e.pArgs.LocationNative;
                                if (this._SelectedEditManifoldPointStart != null && this._SelectedEditManifoldPointEnd != null)
                                {
                                    //Move Geom the difference in meters Horizontally and Vertically
                                    moveGeomWithSQL();
                                    bPrepareEdit = true;
                                }
                                //After MouseUp Event - remove selection for move so that it dosnt move anymore
                                this._SelectedEditManifoldID = -1;
                            }
                            break;
                        default:
                            break;
                        //}
                    }
                    if (this._DrawingOriginalEditObject.Selection.Count < 1 || Convert.ToInt32(this._DrawingOriginalEditObject.Selection[0].Record.get_Data("ID").ToString()) != this._SelectedManifoldID)
                    {
                        Utility.setSelection(this._MapControl, this._DrawingOriginalEditObject, this._SelectedManifoldID);
                    }

                    if (bPrepareEdit == true)
                    {
                        prepareEditObject();
                    }
                }
            }
            catch (Exception objEx)
            {
                throw;
            }
            finally
            {
                this._UserMovedCoordinate = false;
                this._UserAddedCoordinate = false;
                this._UserDeletedCoordinate = false;
                this._UserMovedGeom = false;
                this._MouseIsClickedDown = false;
                this._SelectedEditManifoldPointEnd = null;
                this._SelectedEditManifoldPointStart = null;

                this._SelectedPointBranchIndex = -1;
                this._SelectedPointIndex = -1;
            }
        }

        private void MapControl_MouseMoveEvent(object sender, AxManifold.Interop.IComponentControlEvents_MouseMoveEvent e)
        {
            try
            {
                //Move Coordinate
                if (EditModeOperation == MODETYPE_OPERATION.EditModeEnabled && this._MouseIsClickedDown == true)
                {
                    if (this._DrawingOriginalEditObject.Selection.Count < 1 || Convert.ToInt32(this._DrawingOriginalEditObject.Selection[0].Record.get_Data("ID").ToString()) != this._SelectedManifoldID)
                    {
                        Utility.setSelection(this._MapControl, this._DrawingOriginalEditObject, this._SelectedManifoldID);
                    }

                    if (this._SelectedEditManifoldID > 0)
                    {
                        Manifold.Interop.Point objPointScreen = e.pArgs.LocationScreen;
                        objPointScreen.Y += this._YCoordMapClickOffset;
                        objPointScreen = this._MapControl.ScreenToNative(objPointScreen);
                        moveEditPointCoordinate(objPointScreen.X, objPointScreen.Y);
                    }

                    if (this._DrawingOriginalEditObject.Selection.Count < 1 || Convert.ToInt32(this._DrawingOriginalEditObject.Selection[0].Record.get_Data("ID").ToString()) != this._SelectedManifoldID)
                    {
                        Utility.setSelection(this._MapControl, this._DrawingOriginalEditObject, this._SelectedManifoldID);
                    }
                }

                //Move Geom
                if (EditModeOperation == MODETYPE_OPERATION.EditMoveGeom && this._MouseIsClickedDown == true)
                {
                    if (this._DrawingOriginalEditObject.Selection.Count < 1 || Convert.ToInt32(this._DrawingOriginalEditObject.Selection[0].Record.get_Data("ID").ToString()) != this._SelectedManifoldID)
                    {
                        Utility.setSelection(this._MapControl, this._DrawingOriginalEditObject, this._SelectedManifoldID);
                    }

                    if (this._SelectedEditManifoldID > 0)
                    {
                        Manifold.Interop.Point objPointScreen = e.pArgs.LocationScreen;
                        objPointScreen.Y += this._YCoordMapClickOffset;
                        objPointScreen = this._MapControl.ScreenToNative(objPointScreen);
                        moveEditPointCoordinate(objPointScreen.X, objPointScreen.Y);
                    }

                    if (this._DrawingOriginalEditObject.Selection.Count < 1 || Convert.ToInt32(this._DrawingOriginalEditObject.Selection[0].Record.get_Data("ID").ToString()) != this._SelectedManifoldID)
                    {
                        Utility.setSelection(this._MapControl, this._DrawingOriginalEditObject, this._SelectedManifoldID);
                    }
                }
            }
            catch (Exception objEx)
            {
                throw;
            }
        }

        #endregion

        #region Prepare Edit Layer

        /// <summary>
        /// The Edit DRawing layer is only a temporary layer used while in Edit mode. This method removes 
        ///   all previous edit objects from this Layer.
        /// </summary>
        private void clearEditLayer()
        {
            if (this._EditPointsPopulated == true)
            {
                this._DrawingEdit.Clear(false);
                this._EditPointsPopulated = false;
                this._MapControl.Update();
                this._MapControl.Refresh();
            }
        }

        /// <summary>
        /// The Edit DRawing layer is only a temporary layer used while in Edit mode. This method removes 
        ///   all previous edit objects from this Layer.
        /// </summary>
        private void clearEditLayer(CLEAR_EDIT_LAYER enumCLEAR_EDIT_LAYER)
        {
            try
            {
                if (this._EditPointsPopulated == true)
                {
                    StringBuilder sbSQL = new StringBuilder();

                    switch (enumCLEAR_EDIT_LAYER)
                    {
                        case CLEAR_EDIT_LAYER.NON_SNAP_POINTS:
                            sbSQL.AppendLine(@"DELETE FROM [" + this._DrawingEdit.Name + "] WHERE SnapPoint = FALSE");
                            //tempQuery.Text = sbSQL.ToString();
                            //tempQuery.Run();
                            Utility.executeSQL(sbSQL.ToString());
                            break;
                        case CLEAR_EDIT_LAYER.SNAP_POINTS:
                            sbSQL.AppendLine(@"DELETE FROM [" + this._DrawingEdit.Name + "] WHERE SnapPoint = TRUE");
                            //tempQuery.Text = sbSQL.ToString();
                            //tempQuery.Run();
                            Utility.executeSQL(sbSQL.ToString());
                            break;
                        default:
                            //Clear all
                            clearEditLayer();
                            break;
                    }
                    this._DrawingEdit.Refresh();
                    this._EditPointsPopulated = false;
                }
            }
            catch (Exception objEx)
            {
                throw;
            }
        }

        /// <summary>
        /// This method is used to prep and display the object we want to edit on the Edit Drawing layer.
        ///   It is called every time we want to refresh the Original edit object as well as the Edit Drawing 
        ///   object.
        /// </summary>
        private void prepareEditObject()
        {
            try
            {
                if (Utility.isModeTypeEditEnabled(EditModeOperation) == true)
                {
                    //clearEditLayer(CLEAR_EDIT_LAYER.NON_SNAP_POINTS);

                    clearEditLayer();
                    //If nothing selected then select it
                    if (this._DrawingOriginalEditObject.Selection.Count == 0 && _SelectedManifoldID > 0)
                    {
                        Utility.setSelection(this._MapControl, this._DrawingOriginalEditObject, this._SelectedManifoldID);
                    }

                    if (this._DrawingOriginalEditObject.Selection.Count > 0)
                    {
                        //Get Manifold ID of selected Item
                        this._SelectedManifoldID = Convert.ToInt32(this._DrawingOriginalEditObject.Selection[0].Record.get_Data("ID").ToString());

                        //If Line or Area Copy this object to Edit Drawing
                        if (this._DrawingOriginalEditObject.Selection[0].Type == Manifold.Interop.ObjectType.ObjectArea || this._DrawingOriginalEditObject.Selection[0].Type == Manifold.Interop.ObjectType.ObjectLine)
                        {
                            int iItem = this._DrawingOriginalEditObject.ObjectSet.ItemByID(this._SelectedManifoldID);
                            this._OriginalObjectBranchSet = this._DrawingOriginalEditObject.ObjectSet[iItem].get_Geom().get_BranchSet();

                            insertEditObjectInflectionPoints(this._DrawingOriginalEditObject, this._SelectedManifoldID, this._OriginalObjectBranchSet);
                            //insertEditObjectInflectionPoints(this._DrawingOriginalEditObject, this._SelectedManifoldID);

                            //Need to select something in order for Edit Point Theme to take effect
                            int iTempManifoldId = Utility.getManifoldIDFromQuery(this._DrawingEdit, "SELECT MAX([ID]) AS ID FROM [" + this._DrawingEdit.Name + "]");
                            if (iTempManifoldId > 0)
                            {
                                Utility.setSelection(this._MapControl, this._DrawingEdit, iTempManifoldId);
                            }
                            this._DrawingEdit.SelectNone();
                            this._MapControl.Refresh();
                            this._EditPointsPopulated = true;
                        }
                    }
                }
            }
            catch (Exception objEx)
            {
                throw;
            }
            finally
            {
                //this._UserMovedCoordinate = false;
            }
        }

        #endregion

        #region Move Coordinate

        /// <summary>
        /// Used to move a selected coordinate whilst MODETYPE_OPERATION==EditModeEnabled
        /// </summary>
        /// <param name="p_X"></param>
        /// <param name="p_Y"></param>
        private void moveEditPointCoordinate(double p_X, double p_Y)
        {
            try
            {
                if (EditModeOperation == MODETYPE_OPERATION.EditModeEnabled || EditModeOperation == MODETYPE_OPERATION.EditMoveGeom)
                {
                    if (this._SelectedEditManifoldID > 0)
                    {
                        //Get Point details
                        int iBranchIndex = Convert.ToInt32(this._DrawingEdit.Selection[0].Record.get_Data("BranchIndex").ToString());
                        int iPointIndex = Convert.ToInt32(this._DrawingEdit.Selection[0].Record.get_Data("PointIndex").ToString());
                        movePointCoordinates(this._DrawingEdit, this._SelectedEditManifoldID, p_X, p_Y);
                        this._MapControl.Refresh();
                    }
                }
            }
            catch (Exception objEx)
            {
                throw;
            }
        }

        private void moveOriginalPointCoordinate(double p_X, double p_Y)
        {
            try
            {
                if (EditModeOperation == MODETYPE_OPERATION.EditModeEnabled)
                {
                    if (this._SelectedEditManifoldID > 0)
                    {
                        ////Move the Original object point
                        int iBranchIndex = Convert.ToInt32(this._DrawingEdit.Selection[0].Record.get_Data("BranchIndex").ToString());
                        int iPointIndex = Convert.ToInt32(this._DrawingEdit.Selection[0].Record.get_Data("PointIndex").ToString());

                        //Manifold.Interop.Branch objBranch = VBManifoldWrapper.ManifoldObjectWrapper.getBranch(this._OriginalObjectBranchSet, iBranchIndex);
                        Manifold.Interop.Branch objBranch = this._OriginalObjectBranchSet.get_Item(iBranchIndex);
                        Manifold.Interop.PointSet objPointSet = objBranch.get_PointSet();

                        Manifold.Interop.Point objPoint = VBManifoldWrapper.ManifoldObjectWrapper.getPointFromPointSet(objPointSet, iPointIndex);
                        objPoint.X = p_X;
                        objPoint.Y = p_Y;

                        this._UserMovedCoordinate = true;
                    }
                }
            }
            catch (Exception objEx)
            {
                throw;
            }
        }

        private void movePointCoordinates(Manifold.Interop.Drawing p_Drawing, int p_ManifoldID, double p_X, double p_Y)
        {
            try
            {
                Manifold.Interop.Table objTable = (Manifold.Interop.Table)p_Drawing.OwnedTable;
                StringBuilder sbSQL = new StringBuilder();

                sbSQL.AppendLine("UPDATE [" + p_Drawing.Name + "] ");
                sbSQL.AppendLine("SET [Geom (I)] = AssignCoordSys(NewPoint(" + p_X + ", " + p_Y + "),CoordSys(\"" + p_Drawing.Name + "\" as COMPONENT))");
                sbSQL.AppendLine("WHERE ID = " + p_ManifoldID.ToString());
                Utility.executeSQL(sbSQL.ToString());

                this._UserMovedCoordinate = true;
                this._UserMovedGeom = true;
            }
            catch (Exception objEx)
            {
                throw;
            }
        }

        #endregion

        #region Add Coodinate

        /// <summary>
        /// Adds a coordinate to the selected Edit object
        /// </summary>
        private void addCoordinate1(Manifold.Interop.Point p_objPointScreen)
        {
            Manifold.Interop.Query query;
            bool bAddPointAtEnd = false;
            this._AddingCoordinate = true;
            try
            {
                bool bAdditionOccured = false;
                if (this._DrawingOriginalEditObject.Selection.Count > 0)
                {
                    StringBuilder sbSQL = new StringBuilder();
                    double SearchRange = this._SelectionTolerance; ;
                    double AddCoordinateSelectionTolerance = this._SelectionToleranceAddCoordinate;

                    query = this._MapControl.get_Document().NewQuery("TempQuery", false);

                    //Compose a rectangular area around the clicked point - then gets its Geom WKT
                    Manifold.Interop.PointSet points = this._MapControl.Application.NewPointSet();
                    points.Add(this._MapControl.Application.NewPoint(p_objPointScreen.X - SearchRange, p_objPointScreen.Y - SearchRange));
                    points.Add(this._MapControl.Application.NewPoint(p_objPointScreen.X + SearchRange, p_objPointScreen.Y - SearchRange));
                    points.Add(this._MapControl.Application.NewPoint(p_objPointScreen.X + SearchRange, p_objPointScreen.Y + SearchRange));
                    points.Add(this._MapControl.Application.NewPoint(p_objPointScreen.X - SearchRange, p_objPointScreen.Y + SearchRange));
                    points.Add(VBManifoldWrapper.ManifoldObjectWrapper.getPointFromPointSet(points, 0));

                    Manifold.Interop.Geom geom = this._MapControl.Application.NewGeom(Manifold.Interop.GeomType.GeomArea, null);
                    Manifold.Interop.BranchSet geomBranchSet = geom.get_BranchSet();
                    VBManifoldWrapper.ManifoldObjectWrapper.setPointSetInBranchSet(points, geomBranchSet);
                    string wkt = geom.ToTextWKT();

                    //Create a temp Drawing to hold a copy of the object which will be Exploded into its separate Line segments
                    Manifold.Interop.Drawing tempDrawing = this._MapControl.get_Document().NewDrawing("tempDrawing", this._MapControl.Application.NewCoordinateSystem("Irish Grid"), false);
                    this._DrawingOriginalEditObject.Copy(true);
                    tempDrawing.Paste(true);

                    //NOTE: When the object is exploded the Points at the end of the lines dont match exactely (percision
                    //  is slightly off) to those that were added from the objects PointSet to the Edit Drawing layer thus
                    //  query below adds small BUFFER and TOUCHES logic to get around this.
                    Manifold.Interop.Analyzer myAnalyzer = this._MapControl.get_Document().NewAnalyzer();
                    myAnalyzer.Boundaries((Manifold.Interop.Component)tempDrawing, tempDrawing, tempDrawing.ObjectSet);
                    tempDrawing.SelectInverse();
                    tempDrawing.Clear(true);
                    myAnalyzer.Explode((Manifold.Interop.Component)tempDrawing, tempDrawing.ObjectSet);

                    sbSQL.Length = 0;
                    sbSQL.AppendLine("SELECT [BranchIndex], [PointIndex], [BranchMaxIndex] FROM [" + this._DrawingEdit.Name + "] ");
                    sbSQL.AppendLine("Where TOUCHES([ID],BUFFER( ");
                    sbSQL.AppendLine("Select StartPoint(Geom(ID)) as s FROM [" + tempDrawing.Name + "] ");
                    sbSQL.AppendLine("WHERE Touches(AssignCoordSys(CGeom(CGeomWKB(\"" + wkt + "\")), CoordSys(\"" + this._DrawingEdit.Name + "\" AS COMPONENT)), [ID])," + AddCoordinateSelectionTolerance.ToString() + ")) ");
                    sbSQL.AppendLine("UNION ");
                    sbSQL.AppendLine("SELECT [BranchIndex], [PointIndex], [BranchMaxIndex] FROM [" + this._DrawingEdit.Name + "]  ");
                    sbSQL.AppendLine("Where TOUCHES([ID],BUFFER( ");
                    sbSQL.AppendLine("Select EndPoint(Geom(ID)) as s FROM [" + tempDrawing.Name + "] ");
                    sbSQL.AppendLine("WHERE Touches(AssignCoordSys(CGeom(CGeomWKB(\"" + wkt + "\")), CoordSys(\"" + this._DrawingEdit.Name + "\" AS COMPONENT)), [ID])," + AddCoordinateSelectionTolerance.ToString() + ")) ");
                    sbSQL.AppendLine("ORDER BY [PointIndex] ASC ");

                    query.Text = sbSQL.ToString();
                    query.Run();
                    Manifold.Interop.Table table = query.Table;

                    Int32 iBranchIndex = 0;
                    Int32 iFirstPointIndex = 0;
                    Int32 iLastPointIndex;
                    Int32 iBranchMaxIndex;

                    if (table.RecordSet.Count > 0)
                    {
                        iBranchIndex = Convert.ToInt32(table.RecordSet[0].get_Data("BranchIndex"));
                        iBranchMaxIndex = Convert.ToInt32(table.RecordSet[0].get_Data("BranchMaxIndex"));
                        iFirstPointIndex = Convert.ToInt32(table.RecordSet[0].get_Data("PointIndex"));
                        iLastPointIndex = Convert.ToInt32(table.RecordSet[1].get_Data("PointIndex"));

                        if (iFirstPointIndex != iLastPointIndex)
                        {
                            Manifold.Interop.Branch objBranch = VBManifoldWrapper.ManifoldObjectWrapper.getBranch(this._OriginalObjectBranchSet, iBranchIndex);
                            Manifold.Interop.PointSet objPointSetBefore = objBranch.get_PointSet();
                            Manifold.Interop.PointSet objPointSetAfter = this._MapControl.get_Document().Application.NewPointSet();

                            //Remove Query and tempDrawing
                            tempDrawing.Document.ComponentSet.Remove("tempDrawing");
                            this._DrawingOriginalEditObject.Document.ComponentSet.Remove(query);

                            //When adding point to end of GEOM i.e.- FirstIndex=0 and LastIndex=PointSet.Count
                            // this dosnt work so dont add any points - this will bve done later.
                            if (iFirstPointIndex == 0 && iLastPointIndex == objPointSetBefore.Count - 1)
                            {
                                bAddPointAtEnd = true;
                            }
                            else
                            {
                                //Determine where to insert the point
                                for (int iIndex = 0; iIndex < objPointSetBefore.Count; iIndex++)
                                {
                                    if (bAdditionOccured == false)
                                    {
                                        if (iIndex > iFirstPointIndex)
                                        {
                                            objPointSetAfter.Add(p_objPointScreen);
                                            bAdditionOccured = true;
                                        }
                                    }
                                    objPointSetAfter.Add(VBManifoldWrapper.ManifoldObjectWrapper.getPointFromPointSet(objPointSetBefore, iIndex));
                                }
                            }

                            //Redraw the new Object with added Point if required
                            if (bAdditionOccured == true)
                            {
                                objBranch.set_PointSet(objPointSetAfter);
                                prepareEditObject();
                            }
                            else
                            {
                                if (bAddPointAtEnd == true)
                                {
                                    objPointSetBefore.Add(p_objPointScreen);
                                    objBranch.set_PointSet(objPointSetBefore);
                                    prepareEditObject();
                                }
                            }
                        }
                    }
                    //Unset any Edit Drawing selection
                    this._DrawingEdit.SelectNone();
                    this._UserAddedCoordinate = true;
                }
            }
            catch (Exception objEx)
            {
                throw;
            }
            finally
            {
                this._AddingCoordinate = false;
            }
        }

        /// <summary>
        /// Adds a coordinate to the selected Edit object
        /// </summary>
        private void addCoordinate(Manifold.Interop.Point p_objPointScreen)
        {
            Manifold.Interop.Query query = this._MapControl.get_Document().NewQuery("TempQuery", false);
            bool bAddPointAtEnd = false;
            this._AddingCoordinate = true;
            try
            {
                Manifold.Interop.Drawing tempDrawing = null;
                bool bAdditionOccured = false;
                if (this._DrawingOriginalEditObject.Selection.Count > 0)
                {
                    StringBuilder sbSQL = new StringBuilder();
                    double SearchRange = this._SelectionTolerance; ;
                    double AddCoordinateSelectionTolerance = this._SelectionToleranceAddCoordinate;

                    //Compose a rectangular area around the clicked point - then gets its Geom WKT for use later. This will
                    // be used to in selection later
                    string wkt = Utility.getBoundingBoxWKT(this._DrawingEdit, p_objPointScreen, SearchRange);

                    //Create or use existing Temp Drawing which will hold the Area\Line object that we are adding 
                    // the new Point to. The Area\Line object is Exploded into its separate Line segments
                    int iExists = this._MapControl.get_Document().ComponentSet.ItemByName("TempDrawing");
                    if (iExists > 0)
                    {
                        tempDrawing = (Manifold.Interop.Drawing)this._MapControl.get_Document().ComponentSet["TempDrawing"];
                        tempDrawing.Clear(false);
                    }
                    else
                    {
                        //Create a temp Drawing to hold a copy of the object which will be Exploded into its separate Line segments
                        tempDrawing = this._MapControl.get_Document().NewDrawing("TempDrawing", this._MapControl.Application.NewCoordinateSystem("Irish Grid"), false);
                    }
                    StringBuilder sbSQLExplode = new StringBuilder();
                    sbSQLExplode.AppendLine("INSERT INTO [" + tempDrawing.Name + "] ");
                    sbSQLExplode.AppendLine("([Geom (I)])");
                    sbSQLExplode.AppendLine("SELECT [Segment] FROM [" + this._DrawingOriginalEditObject.Name + "] WHERE ID = " + this._SelectedManifoldID);
                    sbSQLExplode.AppendLine("SPLIT BY Branches(IntersectLine(ConvertToLine([Geom (I)]), [Geom (I)])) AS [Segment]");
                    Utility.executeSQL(sbSQLExplode.ToString());

                    ////===================================================================
                    //// Here I attempted to explode and select in 1 SQL statement.
                    ////===================================================================
                    //StringBuilder ExplodeSQL = new StringBuilder();
                    //ExplodeSQL.AppendLine("   ");
                    //ExplodeSQL.AppendLine("   SELECT [Segment] FROM [" + this._DrawingOriginalEditObject.Name + "] AS [tblExploded] WHERE ID = " + this._SelectedManifoldID);
                    //ExplodeSQL.AppendLine("   SPLIT BY Branches(IntersectLine(ConvertToLine([Geom (I)]), [Geom (I)])) AS [Segment]");

                    //sbSQL.Length = 0;
                    //sbSQL.AppendLine("SELECT [BranchIndex], [PointIndex], [BranchMaxIndex] FROM [" + this._DrawingEdit.Name + "] ");
                    //sbSQL.AppendLine("Where TOUCHES([ID],BUFFER( ");
                    //sbSQL.AppendLine("Select StartPoint(Geom(ID)) as s FROM (" + ExplodeSQL.ToString() + ") ");
                    //sbSQL.AppendLine("WHERE Touches(AssignCoordSys(CGeom(CGeomWKB(\"" + wkt + "\")), CoordSys(\"" + this._DrawingEdit.Name + "\" AS COMPONENT)), [ID])," + AddCoordinateSelectionTolerance.ToString() + ")) ");
                    //sbSQL.AppendLine("OR ");
                    //sbSQL.AppendLine("TOUCHES([ID],BUFFER( ");
                    //sbSQL.AppendLine("Select EndPoint(Geom(ID)) as s FROM (" + ExplodeSQL.ToString() + ") ");
                    //sbSQL.AppendLine("WHERE Touches(AssignCoordSys(CGeom(CGeomWKB(\"" + wkt + "\")), CoordSys(\"" + this._DrawingEdit.Name + "\" AS COMPONENT)), [ID])," + AddCoordinateSelectionTolerance.ToString() + ")) ");
                    //sbSQL.AppendLine("ORDER BY [PointIndex] ASC ");
                    ////===================================================


                    ////This working SQL uses a OR operator - would also be possible using UNION operator.
                    //Using the bounding box WKT and the Exploded object held in the TempDrawing find where the new point should be added.
                    //QUESTION: It should be possible to do away with the 'tempDrawing' and instead combine the above Exploded SQL and the
                    //          SQL below into 1 query.
                    sbSQL.Length = 0;
                    sbSQL.AppendLine("SELECT [BranchIndex], [PointIndex], [BranchMaxIndex] FROM [" + this._DrawingEdit.Name + "] ");
                    sbSQL.AppendLine("Where TOUCHES([ID],BUFFER( ");
                    sbSQL.AppendLine("Select StartPoint(Geom(ID)) as s FROM [" + tempDrawing.Name + "] ");
                    sbSQL.AppendLine("WHERE Touches(AssignCoordSys(CGeom(CGeomWKB(\"" + wkt + "\")), CoordSys(\"" + this._DrawingEdit.Name + "\" AS COMPONENT)), [ID])," + AddCoordinateSelectionTolerance.ToString() + ")) ");
                    sbSQL.AppendLine("OR ");
                    sbSQL.AppendLine("TOUCHES([ID],BUFFER( ");
                    sbSQL.AppendLine("Select EndPoint(Geom(ID)) as s FROM [" + tempDrawing.Name + "] ");
                    sbSQL.AppendLine("WHERE Touches(AssignCoordSys(CGeom(CGeomWKB(\"" + wkt + "\")), CoordSys(\"" + this._DrawingEdit.Name + "\" AS COMPONENT)), [ID])," + AddCoordinateSelectionTolerance.ToString() + ")) ");
                    sbSQL.AppendLine("ORDER BY [PointIndex] ASC ");

                    query.Text = sbSQL.ToString();
                    query.Run();
                    Manifold.Interop.Table table = query.Table;

                    Int32 iBranchIndex = 0;
                    Int32 iFirstPointIndex = 0;
                    Int32 iLastPointIndex;
                    Int32 iBranchMaxIndex;

                    if (table.RecordSet.Count > 0)
                    {
                        iBranchIndex = Convert.ToInt32(table.RecordSet[0].get_Data("BranchIndex"));
                        iBranchMaxIndex = Convert.ToInt32(table.RecordSet[0].get_Data("BranchMaxIndex"));
                        iFirstPointIndex = Convert.ToInt32(table.RecordSet[0].get_Data("PointIndex"));
                        iLastPointIndex = Convert.ToInt32(table.RecordSet[1].get_Data("PointIndex"));

                        if (iFirstPointIndex != iLastPointIndex)
                        {
                            Manifold.Interop.Branch objBranch = VBManifoldWrapper.ManifoldObjectWrapper.getBranch(this._OriginalObjectBranchSet, iBranchIndex);
                            Manifold.Interop.PointSet objPointSetBefore = objBranch.get_PointSet();
                            Manifold.Interop.PointSet objPointSetAfter = this._MapControl.get_Document().Application.NewPointSet();

                            //Remove Query and tempDrawing
                            tempDrawing.Document.ComponentSet.Remove("tempDrawing");
                            this._DrawingOriginalEditObject.Document.ComponentSet.Remove(query);

                            //When adding point to end of GEOM i.e.- FirstIndex=0 and LastIndex=PointSet.Count
                            // this dosnt work so dont add any points - this will bve done later.
                            if (iFirstPointIndex == 0 && iLastPointIndex == objPointSetBefore.Count - 1)
                            {
                                bAddPointAtEnd = true;
                            }
                            else
                            {
                                //Determine where to insert the point
                                for (int iIndex = 0; iIndex < objPointSetBefore.Count; iIndex++)
                                {
                                    if (bAdditionOccured == false)
                                    {
                                        if (iIndex > iFirstPointIndex)
                                        {
                                            objPointSetAfter.Add(p_objPointScreen);
                                            bAdditionOccured = true;
                                        }
                                    }
                                    objPointSetAfter.Add(VBManifoldWrapper.ManifoldObjectWrapper.getPointFromPointSet(objPointSetBefore, iIndex));
                                }
                            }

                            //Redraw the new Object with added Point if required
                            if (bAdditionOccured == true)
                            {
                                objBranch.set_PointSet(objPointSetAfter);
                                prepareEditObject();
                            }
                            else
                            {
                                if (bAddPointAtEnd == true)
                                {
                                    objPointSetBefore.Add(p_objPointScreen);
                                    objBranch.set_PointSet(objPointSetBefore);
                                    prepareEditObject();
                                }
                            }
                        }
                    }
                    //Unset any Edit Drawing selection
                    this._DrawingEdit.SelectNone();
                    this._UserAddedCoordinate = true;
                }
            }
            catch (Exception objEx)
            {
                throw;
            }
            finally
            {
                this._AddingCoordinate = false;
            }
        }

        #endregion

        #region Delete Coordinate

        /// <summary>
        /// Used to delete a selected coordinate whilst MODETYPE_OPERATION==DeleteCoordinate
        /// </summary>
        private void deleteCoordinate(Manifold.Interop.Point p_objPointScreen)
        {
            try
            {
                if (EditModeOperation == MODETYPE_OPERATION.EditDeleteCoordinate)
                {
                    Utility.setNearestSelection(this._MapControl, this._DrawingEdit, p_objPointScreen, this._SelectionTolerance);
                    if (this._DrawingEdit.Selection.Count > 0)
                    {
                        switch (this._DrawingOriginalEditObject.Selection[0].Type)
                        {
                            case Manifold.Interop.ObjectType.ObjectArea:
                                deleteAreaCoordinate();
                                break;
                            case Manifold.Interop.ObjectType.ObjectLine:
                                deleteLineCoordinate();
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (Exception objEx)
            {
                throw;
            }
        }

        private void deleteLineCoordinate()
        {
            try
            {
                bool bDeleteOccured = false;
                if (this._DrawingEdit.Selection.Count > 0)
                {
                    int iBranchIndex = Convert.ToInt32(this._DrawingEdit.Selection[0].Record.get_Data("BranchIndex").ToString());
                    int iPointIndex = Convert.ToInt32(this._DrawingEdit.Selection[0].Record.get_Data("PointIndex").ToString());

                    Manifold.Interop.Branch objBranch = VBManifoldWrapper.ManifoldObjectWrapper.getBranch(this._OriginalObjectBranchSet, iBranchIndex);
                    Manifold.Interop.PointSet objPointSet = objBranch.get_PointSet();

                    //Delete the point - but only if there are more than 2 coordinates
                    //NOTE: My Lines only have 1 branch
                    if (objPointSet.Count > 2)
                    {
                        objPointSet.Remove(iPointIndex);
                        bDeleteOccured = true;
                    }
                    else
                    {
                        //If the is more than 1 branch then delete the entire branch
                        if (this._OriginalObjectBranchSet.Count > 1)
                        {
                            this._OriginalObjectBranchSet.Remove(iBranchIndex);
                            bDeleteOccured = true;
                        }
                    }

                    if (bDeleteOccured == true)
                    {
                        //Refresh surfaces
                        prepareEditObject();

                        this._UserDeletedCoordinate = true;

                        this._MapControl.Update();
                        this._MapControl.Refresh();
                    }
                }
            }

            catch (Exception objEx)
            {
                throw;
            }
        }

        private void deleteAreaCoordinate()
        {
            try
            {
                bool bDeleteOccured = false;
                if (this._DrawingEdit.Selection.Count > 0)
                {
                    int iBranchIndex = Convert.ToInt32(this._DrawingEdit.Selection[0].Record.get_Data("BranchIndex").ToString());
                    int iPointIndex = Convert.ToInt32(this._DrawingEdit.Selection[0].Record.get_Data("PointIndex").ToString());

                    Manifold.Interop.Branch objBranch = VBManifoldWrapper.ManifoldObjectWrapper.getBranch(this._OriginalObjectBranchSet, iBranchIndex);
                    Manifold.Interop.PointSet objPointSet = objBranch.get_PointSet();

                    //Delete the point - but only if the Pointset is more than 3 Points otherwise delete the entire branch (if there is more than 1 branch)
                    if (objPointSet.Count > 3)
                    {
                        objPointSet.Remove(iPointIndex);
                        bDeleteOccured = true;
                    }
                    else
                    {
                        //If the is more than 1 branch then delete the entire branch
                        if (this._OriginalObjectBranchSet.Count > 1)
                        {
                            this._OriginalObjectBranchSet.Remove(iBranchIndex);
                            bDeleteOccured = true;
                        }
                    }

                    if (bDeleteOccured == true)
                    {
                        //Refresh surfaces
                        prepareEditObject();
                        this._UserDeletedCoordinate = true;

                        this._MapControl.Update();
                        this._MapControl.Refresh();
                    }
                }
            }
            catch (Exception objEx)
            {
                throw;
            }
        }

        #endregion

        #region Move Geom

        /// <summary>
        /// If you use the Analyzer object to move the GEOM then the GEOM is assigned a new Manifold ID every time 
        /// a Transform is used e.g.MoveHorizontally.
        /// </summary>
        private void moveGeomWithAnalyzer()
        {

            try
            {
                double dblStartX = this._SelectedEditManifoldPointStart.X;
                double dblEndX = this._SelectedEditManifoldPointEnd.X;
                double moveHorizontally = dblEndX - dblStartX;

                double dblStartY = this._SelectedEditManifoldPointStart.Y;
                double dblEndY = this._SelectedEditManifoldPointEnd.Y;
                double moveVertically = dblEndY - dblStartY;

                Manifold.Interop.Analyzer analyzer = this._MapControl.get_Document().NewAnalyzer();
                //Move by meters
                object unit = this.DrawingEdit.CoordinateSystem.Unit;

                this._SelectedManifoldID = analyzer.MoveHorizontally((Manifold.Interop.Component)this.DrawingOriginalEditObject, this.DrawingOriginalEditObject.Selection, moveHorizontally, unit)[0].ID;
                this._SelectedManifoldID = analyzer.MoveVertically((Manifold.Interop.Component)this.DrawingOriginalEditObject, this.DrawingOriginalEditObject.Selection, moveVertically, unit)[0].ID;
            }
            catch (Exception err)
            {
                throw;
            }
        }

        /// <summary>
        /// If you use the Analyzer object to move the GEOM then the GEOM is assigned a new Manifold ID every time 
        /// a Transform is used e.g.MoveHorizontally. However doing this by SQL does not require new ID.
        /// </summary>
        protected void moveGeomWithSQL()
        {
            try
            {
                //double dblStartX = this._SelectedEditManifoldPointStart.X;
                //double dblEndX = this._SelectedEditManifoldPointEnd.X;
                //double moveHorizontally = dblEndX - dblStartX;

                //double dblStartY = this._SelectedEditManifoldPointStart.Y;
                //double dblEndY = this._SelectedEditManifoldPointEnd.Y;
                //double moveVertically = dblEndY - dblStartY;

                //Utility.UtilityDrawing.moveGeomWithSQL(this.DrawingOriginalEditObject,moveHorizontally,moveVertically,this._SelectedManifoldID);

                this._GeomMovedDetails = Utility.moveGeomWithSQL(this.DrawingOriginalEditObject, this._SelectedEditManifoldPointStart, this._SelectedEditManifoldPointEnd, this._SelectedManifoldID);
                this._UserMovedGeom = true;
            }
            catch (Exception err)
            {
                throw;
            }
        }
        #endregion

        public void Dispose()
        {
            try
            {
                Dispose(true);
                // This object will be cleaned up by the Dispose method.
                // Therefore, you should call GC.SupressFinalize to
                // take this object off the finalization queue 
                // and prevent finalization code for this object
                // from executing a second time.
                GC.SuppressFinalize(this);
            }
            catch (Exception err)
            {
                throw;
            }
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be _DISPOSED.
        // If disposing equals false, the method has been called by the 
        // runtime from inside the finalizer and you should not reference 
        // other objects. Only unmanaged resources can be _DISPOSED.
        private void Dispose(bool disposing)
        {
            try
            {
                // Check to see if Dispose has already been called.
                if (!this._DISPOSED)
                {
                    // If disposing equals true, dispose all managed 
                    // and unmanaged resources.
                    if (disposing)
                    {
                        this._MapControl.MouseDownEvent -= new AxManifold.Interop.IComponentControlEvents_MouseDownEventHandler(this.MapControl_MouseDownEvent);
                        this._MapControl.MouseUpEvent -= new AxManifold.Interop.IComponentControlEvents_MouseUpEventHandler(this.MapControl_MouseUpEvent);
                        this._MapControl.MouseMoveEvent -= new AxManifold.Interop.IComponentControlEvents_MouseMoveEventHandler(this.MapControl_MouseMoveEvent);
                    }
                }
                _DISPOSED = true;
            }
            catch (Exception err)
            {
                throw;
            }
        }

        private void insertEditObjectInflectionPoints(Manifold.Interop.Drawing p_Drawing, int p_ManifoldID)
        {
            try
            {
                Manifold.Interop.Table objTable = (Manifold.Interop.Table)p_Drawing.OwnedTable;
                StringBuilder sbSQL = new StringBuilder();

                sbSQL.AppendLine("INSERT INTO [" + this._DrawingEdit.Name + "] ");
                sbSQL.AppendLine("( ");
                sbSQL.AppendLine("  PointIndex ");
                sbSQL.AppendLine("  ,[Geom (I)] ");
                sbSQL.AppendLine("  ,BranchMaxIndex ");
                sbSQL.AppendLine("  ,ThemeFeatureType ");
                sbSQL.AppendLine("  ,[Selection (I)]");
                sbSQL.AppendLine(") ");
                sbSQL.AppendLine("SELECT ");
                sbSQL.AppendLine("  CASE [Point] = StartPoint(ConvertToLine([ID])) ");
                sbSQL.AppendLine("    WHEN TRUE THEN 0 ");
                sbSQL.AppendLine("    ELSE CoordCount(Branch(IntersectLine(ConvertToLine([ID]), [Point]), 0)) - 1 ");
                sbSQL.AppendLine("  END AS [PointIndex] ");
                sbSQL.AppendLine("  ,[Point] ");
                sbSQL.AppendLine("  ,CoordCount(ID) AS [BranchPointMaxIndex] ");
                sbSQL.AppendLine("  , \"" + this._Edit_ThemePoint + "\"");
                sbSQL.AppendLine("  , 1");
                sbSQL.AppendLine("FROM [" + p_Drawing.Name + "]  ");
                sbSQL.AppendLine("WHERE ID = " + p_ManifoldID.ToString() + " ");
                sbSQL.AppendLine("SPLIT BY Coords([ID]) AS [Point]; ");

                Utility.executeSQL(sbSQL.ToString());
            }
            catch (Exception objEx)
            {
                throw;
            }
        }

        private void insertEditObjectInflectionPoints(Manifold.Interop.Drawing p_Drawing, int p_ManifoldID, Manifold.Interop.BranchSet p_ManifoldObjectBranchSet)
        {
            try
            {
                StringBuilder sbSQL = new StringBuilder();
                for (int iBranchIndex = 0; iBranchIndex < p_ManifoldObjectBranchSet.Count; iBranchIndex++)
                {
                    Manifold.Interop.Branch objBranch = p_ManifoldObjectBranchSet.get_Item(iBranchIndex);
                    int iCoorCounts = objBranch.get_PointSet().Count;

                    sbSQL.Length = 0;
                    sbSQL.AppendLine("INSERT INTO [" + this._DrawingEdit.Name + "] ");
                    sbSQL.AppendLine("( ");
                    sbSQL.AppendLine("  PointIndex ");
                    sbSQL.AppendLine("  ,[Geom (I)] ");
                    sbSQL.AppendLine("  ,BranchMaxIndex ");
                    sbSQL.AppendLine("  ,ThemeFeatureType ");
                    sbSQL.AppendLine("  ,[Selection (I)]");
                    sbSQL.AppendLine("  ,BranchIndex ");
                    sbSQL.AppendLine(") ");
                    sbSQL.AppendLine("SELECT ");
                    sbSQL.AppendLine("  CASE [Point] = StartPoint(ConvertToLine(BRANCH([ID]," + iBranchIndex.ToString() + "))) ");
                    sbSQL.AppendLine("    WHEN TRUE THEN 0 ");
                    sbSQL.AppendLine("    ELSE CoordCount(Branch(IntersectLine(ConvertToLine(BRANCH([ID]," + iBranchIndex.ToString() + ")), [Point]), 0)) - 1 ");
                    sbSQL.AppendLine("  END AS [PointIndex] ");
                    sbSQL.AppendLine("  ,[Point] ");
                    sbSQL.AppendLine("  ,CoordCount(BRANCH([ID]," + iBranchIndex.ToString() + ")) AS [BranchPointMaxIndex] ");
                    sbSQL.AppendLine("  , \"" + this._Edit_ThemePoint + "\"");
                    sbSQL.AppendLine("  , 1");
                    sbSQL.AppendLine("  , " + iBranchIndex.ToString() + " ");
                    sbSQL.AppendLine("FROM [" + p_Drawing.Name + "]  ");
                    sbSQL.AppendLine("WHERE ID = " + p_ManifoldID.ToString() + " ");
                    sbSQL.AppendLine("SPLIT BY Coords(BRANCH([ID]," + iBranchIndex.ToString() + ")) AS [Point]; ");
                    Utility.executeSQL(sbSQL.ToString());
                }
            }
            catch (Exception objEx)
            {
                throw;
            }
        }

        private void insertEditObjectInflectionPoints1(Manifold.Interop.Drawing p_Drawing, int p_ManifoldID, Manifold.Interop.BranchSet p_ManifoldObjectBranchSet)
        {
            Manifold.Interop.Query qryQuerySelect = p_Drawing.Document.NewQuery("Temp_EditSelect", false);
            Manifold.Interop.Query qryQueryInsert = p_Drawing.Document.NewQuery("Temp_EditInsert", false);
            try
            {
                StringBuilder sbSQL = new StringBuilder();


                for (int iBranchIndex = 0; iBranchIndex < p_ManifoldObjectBranchSet.Count; iBranchIndex++)
                {
                    Manifold.Interop.Branch objBranch = p_ManifoldObjectBranchSet.get_Item(iBranchIndex);
                    int iCoorCounts = objBranch.get_PointSet().Count;

                    sbSQL.Length = 0;
                    sbSQL.AppendLine("SELECT ");
                    sbSQL.AppendLine("  [ID],  ");
                    sbSQL.AppendLine("  [Geom (I)],  ");
                    sbSQL.AppendLine("  [Vertex], ");
                    sbSQL.AppendLine("  Length([Section]) AS [Length] ");
                    sbSQL.AppendLine("FROM ");
                    sbSQL.AppendLine("  (SELECT  ");
                    sbSQL.AppendLine("    [ID], [Geom (I)], [Vertex], ");
                    sbSQL.AppendLine("    IntersectLine(ReverseLine(ConvertToLine(BRANCH([ID]," + iBranchIndex.ToString() + "))), [Vertex]) AS [Sections] ");
                    sbSQL.AppendLine("  FROM [" + p_Drawing.Name + "] ");
                    sbSQL.AppendLine("  WHERE [ID] = " + p_ManifoldID.ToString() + " ");
                    sbSQL.AppendLine("  SPLIT BY Coords(BRANCH([ID]," + iBranchIndex.ToString() + ")) AS [Vertex] ");
                    sbSQL.AppendLine("  ) ");
                    sbSQL.AppendLine("SPLIT BY Branches([Sections]) AS [Section] ");
                    sbSQL.AppendLine("LEAVING Touches(StartPoint([Section]), StartPoint(ConvertToLine(BRANCH([ID]," + iBranchIndex.ToString() + ")))) ");
                    qryQuerySelect.Text = sbSQL.ToString();
                    qryQuerySelect.RunEx(false);

                    sbSQL.Length = 0;
                    sbSQL.AppendLine("INSERT INTO [" + this._DrawingEdit.Name + "] ");
                    sbSQL.AppendLine("( ");
                    sbSQL.AppendLine("  PointIndex ");
                    sbSQL.AppendLine("  ,[Geom (I)] ");
                    sbSQL.AppendLine("  ,ThemeFeatureType ");
                    sbSQL.AppendLine("  ,[Selection (I)] ");
                    sbSQL.AppendLine("  ,BranchIndex ");
                    sbSQL.AppendLine(") ");
                    sbSQL.AppendLine("SELECT ");
                    sbSQL.AppendLine("    COUNT([T2].[Length]) ");
                    sbSQL.AppendLine("    ,[T1].[Vertex] ");
                    sbSQL.AppendLine("  , \"" + this._Edit_ThemePoint + "\"");
                    sbSQL.AppendLine("	, 1 ");
                    sbSQL.AppendLine("  , " + iBranchIndex.ToString() + " ");
                    sbSQL.AppendLine("FROM ");
                    sbSQL.AppendLine("    [" + qryQuerySelect.Name + "] AS [T1] ");
                    sbSQL.AppendLine("    LEFT JOIN ");
                    sbSQL.AppendLine("    [" + qryQuerySelect.Name + "] AS [T2] ");
                    sbSQL.AppendLine("   ON [T1].[ID] = [T2].[ID] ");
                    sbSQL.AppendLine("    AND [T1].[Length] < [T2].[Length] ");
                    sbSQL.AppendLine("GROUP BY [T1].[ID], [T1].[Vertex] ");
                    qryQueryInsert.Text = sbSQL.ToString();
                    qryQueryInsert.RunEx(false);
                }
            }
            catch (Exception objEx)
            {
                throw;
            }
            finally
            {
                //Remove the query
                p_Drawing.Document.ComponentSet.Remove(qryQuerySelect);
                p_Drawing.Document.ComponentSet.Remove(qryQueryInsert);
            }
        }

        /// <summary>
        /// When the MODETYPE_OPERATION property of this class is set to 'EditEnabled' then this method
        /// is called to check that the required objects are set appropriately.
        /// </summary>
        private bool validateEditObjects()
        {
            bool bSuccess = true;
            bool bError = false;
            string sErrorMsg = "";
            try
            {
                if (this._DrawingOriginalEditObject == null)
                {
                    bError = true;
                    sErrorMsg = "The Manifold Drawing layer that contains the object to be edited needs to be set before Editing functionality is enabled. Use the 'startEditing()' method to set this parameter.";
                }

                if (this._MapControl == null)
                {
                    bError = true;
                    sErrorMsg = "The Manifold ActiveX MapControl needs to be set before Edit functionality is enabled. Use the class constructor to set this parameter.";
                }

                if (this._DrawingEdit == null)
                {
                    bError = true;
                    sErrorMsg = "The 'Edit Layer' Manifold Drawing layer needs to be set before Editing functionality is enabled. Use the class constructor to set this parameter.";
                }

                //Need to check that the Manifold objects selected are Lines or Areas??
                if (bError == true)
                {
                    bSuccess = false;
                    //OnMapModeTypeChanged(new ModeTypeEventArgs(this._EditModeOperation));
                    this.EditModeOperation = MODETYPE_OPERATION.SelectMode;
                    throw new Exception(sErrorMsg);
                }
            }
            catch (Exception objEx)
            {
                throw;
            }
            return bSuccess;
        }
    }
}
