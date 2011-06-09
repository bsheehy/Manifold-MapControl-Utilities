using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace ManifoldMapEdit
{
    public partial class frmMap : Form
    {
        private Manifold.Interop.Map MapProject;
        private Manifold.Interop.Drawing DrawingEdit = null;
        private Manifold.Interop.Drawing DrawingOriginal = null;
        private EditDrawingFactorySQL _EditFactory;
        private MODETYPE_OPERATION ModeTypeOperation = MODETYPE_OPERATION.SelectMode;
        private bool inEventModeTypeOperationChanged = false;
        private bool _IsDrawing = false;

        public frmMap()
        {
            InitializeComponent();
            LoadMap();
        }

        public void LoadMap()
        {
            try
            {
                string path = Path.GetDirectoryName(Application.ExecutablePath) + @"\Map\K1431158.map";
                this.MapProject = Utility.loadManifoldMap(axComponentControl1, path, "Map");

                DrawingEdit = (Manifold.Interop.Drawing)axComponentControl1.get_Document().ComponentSet["K1431158_Edit"];
                DrawingOriginal = (Manifold.Interop.Drawing)axComponentControl1.get_Document().ComponentSet["K1431158_ParcelInspections"];
                Constants.QUERYDEFAULT = (Manifold.Interop.Query)axComponentControl1.get_Document().ComponentSet[Constants.QUERYDEFAULT_NAME];
                
                this._EditFactory = new EditDrawingFactorySQL(axComponentControl1, DrawingOriginal, DrawingEdit, MODETYPE_OPERATION.SelectMode);
                this._EditFactory.MapModeTypeChanged += new EventHandler<ModeTypeEventArgs>(EventHandler_onEditModeChanged);
            }
            catch (Exception err)
            {
                MessageBox.Show("ERROR:" + err.Message);
            }
        }

        //public void editModeChanged(MODETYPE_OPERATION p_EditMode)
        //{
        //    try
        //    {
        //        //Thread safe method for updating controls -  Thread is not the form thread we have to Invoke the method for thread safety. 
        //        if (IsHandleCreated)
        //        {
        //            lblMode.Invoke((MethodInvoker)delegate
        //            {
        //                this.ModeTypeOperation = p_EditMode;
        //                switch (p_EditMode)
        //                {
        //                    case MODETYPE_OPERATION.EditAddCoordinate:
        //                        lblMode.Text = "Mode: Edit Add Coordinate";
        //                        break;
        //                    case MODETYPE_OPERATION.EditDeleteCoordinate:
        //                        lblMode.Text = "Mode: Edit Delete Coordinate";
        //                        break;
        //                    case MODETYPE_OPERATION.SnapModeEnabled:
        //                        lblMode.Text = "Mode: Edit Snap To Parcel";
        //                        break;
        //                    case MODETYPE_OPERATION.DrawArea:
        //                        lblMode.Text = "Mode: Draw Polygon";
        //                        break;
        //                    case MODETYPE_OPERATION.EditModeEnabled:
        //                        lblMode.Text = "Mode: Edit Move Coordinate";
        //                        break;
        //                    default:
        //                        lblMode.Text = "Mode: Select";
        //                        break;
        //                }
        //            });
        //        }
        //    }
        //    catch (Exception)
        //    {
        //    }
        //    finally
        //    {
        //    }
        //}

        //public void EventHandler_onModeTypeOperationChanged(object sender, ModeTypeEventArgs p_ModeType)
        //{
        //    try
        //    {
        //        if (inEventModeTypeOperationChanged == false)
        //        {
        //            inEventModeTypeOperationChanged = true;
        //            if (this.ModeTypeOperation != p_ModeType.MODETYPE_OPERATION)
        //            {
        //                this.ModeTypeOperation = p_ModeType.MODETYPE_OPERATION;
        //            }

        //            if (this._EditFactory != null)
        //            {
        //                if (this._EditFactory.EditModeOperation != p_ModeType.MODETYPE_OPERATION)
        //                {
        //                    this._EditFactory.OnMapModeTypeChanged(p_ModeType);
        //                }
        //            }

        //            //if (this._SnapFactory != null)
        //            //{
        //            //    if (this._SnapFactory.EditModeOperation != p_ModeType.MODETYPE_OPERATION)
        //            //    {
        //            //        this._SnapFactory.OnMapModeTypeChanged(p_ModeType);
        //            //    }
        //            //}
        //        }
        //    }
        //    catch (Exception objEx)
        //    {
        //        throw;
        //    }
        //    finally
        //    {
        //        inEventModeTypeOperationChanged = false;
        //    }
        //}

        public void EventHandler_onEditModeChanged(object sender, ModeTypeEventArgs changeArgs)
        {
            if (this.ModeTypeOperation != changeArgs.MODETYPE_OPERATION)
            {
                try
                {
                    if (IsHandleCreated)
                    {
                        axComponentControl1.MouseMode = Manifold.Interop.ControlMouseMode.ControlMouseModeNone;
                        this.ModeTypeOperation = changeArgs.MODETYPE_OPERATION;
                        gbEditMode.Text = "  Edit Mode - Disabled:  ";
                        gbDrawControls.Enabled = true;

                        if (this._EditFactory != null)
                        {
                            if (this._EditFactory.EditModeOperation != changeArgs.MODETYPE_OPERATION)
                            {
                                this._EditFactory.OnMapModeTypeChanged(changeArgs);
                            }
                        }

                        switch (changeArgs.MODETYPE_OPERATION)
                        {
                            case MODETYPE_OPERATION.SelectMode:
                                lblMode.Text = "Mode: Select";
                                Utility.unsetSelection(axComponentControl1, DrawingOriginal);
                                break;
                            case MODETYPE_OPERATION.EditModeEnabled:
                                gbDrawControls.Enabled = false;

                                gbEditMode.Text = "  Edit Mode - Enabled:  ";
                                lblMode.Text = "Mode: Edit-Move Coordinate";
                                btnEditAddCoordinate.Enabled = true;
                                btnEditDeleteCoordinate.Enabled = true;
                                btnEditMoveGeom.Enabled = true;
                                break;
                            case MODETYPE_OPERATION.EditMoveGeom:
                                gbDrawControls.Enabled = false;
                                gbEditMode.Text = "  Edit Mode - Move Geom:  ";
                                lblMode.Text = "Mode: Edit-Move Geom";
                                btnEditAddCoordinate.Enabled = true;
                                btnEditDeleteCoordinate.Enabled = true;
                                btnEditMoveGeom.Enabled = true;
                                break;
                            case MODETYPE_OPERATION.EditAddCoordinate:
                                gbDrawControls.Enabled = false;

                                gbEditMode.Text = "  Edit Mode - Add Coordinate:  ";
                                lblMode.Text = "Mode: Edit-Add Coordinate";
                                btnEditAddCoordinate.Enabled = true;
                                btnEditDeleteCoordinate.Enabled = true;
                                btnEditMoveGeom.Enabled = true;
                                break;
                            case MODETYPE_OPERATION.EditDeleteCoordinate:
                                gbDrawControls.Enabled = false;
                                gbEditMode.Text = "  Edit Mode - Delete Coordinate:  ";
                                lblMode.Text = "Mode: Edit-Delete Coordinate";
                                btnEditAddCoordinate.Enabled = true;
                                btnEditDeleteCoordinate.Enabled = true;
                                btnEditMoveGeom.Enabled = true;
                                break;
                            case MODETYPE_OPERATION.DrawArea:
                                axComponentControl1.MouseMode = Manifold.Interop.ControlMouseMode.ControlMouseModeGenericArea;
                                lblMode.Text = "Mode: Draw Area";
                                break;
                            case MODETYPE_OPERATION.DrawLine:
                                axComponentControl1.MouseMode = Manifold.Interop.ControlMouseMode.ControlMouseModeGenericLine;
                                lblMode.Text = "Mode: Draw Line";
                                break;
                            case MODETYPE_OPERATION.DrawPoint:
                                axComponentControl1.MouseMode = Manifold.Interop.ControlMouseMode.ControlMouseModeGenericPoint;
                                lblMode.Text = "Mode: Draw Point";
                                break;
                            default:
                                break;
                        }
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show("ERROR:" + err.Message);
                }
                finally
                {
                }
            }
        }

        private void btnDrawPoint_Click(object sender, EventArgs e)
        {
            if (this.ModeTypeOperation == MODETYPE_OPERATION.DrawPoint)
            {
                EventHandler_onEditModeChanged(this, new ModeTypeEventArgs(MODETYPE_OPERATION.SelectMode));
            }
            else
            {
                EventHandler_onEditModeChanged(this, new ModeTypeEventArgs(MODETYPE_OPERATION.DrawPoint));
            }
        }

        private void btnDrawLine_Click(object sender, EventArgs e)
        {
            if (this.ModeTypeOperation == MODETYPE_OPERATION.DrawLine)
            {
                EventHandler_onEditModeChanged(this, new ModeTypeEventArgs(MODETYPE_OPERATION.SelectMode));
            }
            else
            {
                EventHandler_onEditModeChanged(this, new ModeTypeEventArgs(MODETYPE_OPERATION.DrawLine));
            }
        }

        private void btnDrawArea_Click(object sender, EventArgs e)
        {
            if (this.ModeTypeOperation == MODETYPE_OPERATION.DrawArea)
            {
                EventHandler_onEditModeChanged(this, new ModeTypeEventArgs(MODETYPE_OPERATION.SelectMode));
            }
            else
            {
                EventHandler_onEditModeChanged(this, new ModeTypeEventArgs(MODETYPE_OPERATION.DrawArea));
            }
        }

        private void btnEditEnable_Click(object sender, EventArgs e)
        {
            if (this.ModeTypeOperation == MODETYPE_OPERATION.EditModeEnabled)
            {
                EventHandler_onEditModeChanged(this, new ModeTypeEventArgs(MODETYPE_OPERATION.SelectMode));
            }
            else
            {
                if (this.DrawingOriginal.Selection.Count > 0)
                {
                    EventHandler_onEditModeChanged(this, new ModeTypeEventArgs(MODETYPE_OPERATION.EditModeEnabled));
                }
                else
                {
                    EventHandler_onEditModeChanged(this, new ModeTypeEventArgs(MODETYPE_OPERATION.SelectMode));
                }
            }
        }

        private void btnEditDeleteCoordinate_Click(object sender, EventArgs e)
        {
            if (this.ModeTypeOperation == MODETYPE_OPERATION.EditDeleteCoordinate)
            {
                EventHandler_onEditModeChanged(this, new ModeTypeEventArgs(MODETYPE_OPERATION.EditModeEnabled));
            }
            else
            {
                EventHandler_onEditModeChanged(this, new ModeTypeEventArgs(MODETYPE_OPERATION.EditDeleteCoordinate));
            }
        }

        private void btnEditAddCoordinate_Click(object sender, EventArgs e)
        {
            if (this.ModeTypeOperation == MODETYPE_OPERATION.EditAddCoordinate)
            {
                EventHandler_onEditModeChanged(this, new ModeTypeEventArgs(MODETYPE_OPERATION.EditModeEnabled));
            }
            else
            {
                EventHandler_onEditModeChanged(this, new ModeTypeEventArgs(MODETYPE_OPERATION.EditAddCoordinate));
            }
        }

        private void btnEditMoveGeom_Click(object sender, EventArgs e)
        {
            if (this.ModeTypeOperation == MODETYPE_OPERATION.EditMoveGeom)
            {
                EventHandler_onEditModeChanged(this, new ModeTypeEventArgs(MODETYPE_OPERATION.EditModeEnabled));
            }
            else
            {
                EventHandler_onEditModeChanged(this, new ModeTypeEventArgs(MODETYPE_OPERATION.EditMoveGeom));
            }
        }

        private void axComponentControl1_ClickEvent(object sender, AxManifold.Interop.IComponentControlEvents_ClickEvent e)
        {
            try
            {
                double x, y;
                x = e.pArgs.LocationLatLon.X;
                y = e.pArgs.LocationLatLon.Y;

                if (axComponentControl1.MouseMode == Manifold.Interop.ControlMouseMode.ControlMouseModeNone)
                {
                    switch (e.pArgs.Button)
                    {
                        //HighLight is default selection
                        case Manifold.Interop.ControlMouseButton.MouseButtonLeft:
                            //HACK: adjust vertical location for the control's tool bar
                            Manifold.Interop.Point objPointScreen = e.pArgs.LocationScreen;
                            objPointScreen.Y += 27;
                            mapObjectSelection(objPointScreen);
                            break;
                        case Manifold.Interop.ControlMouseButton.MouseButtonMiddle:
                            break;
                        case Manifold.Interop.ControlMouseButton.MouseButtonRight:
                            this._IsDrawing = false;
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception err)
            {
                MessageBox.Show("ERROR:" + err.Message);
            }
        }

        private void mapObjectSelection(Manifold.Interop.Point p_PointLocationScreen)
        {
            try
            {
                //Clear any Previous selection
                p_PointLocationScreen = axComponentControl1.ScreenToNative(p_PointLocationScreen);
                //Utility.unsetSelection(axComponentControl1,DrawingOriginal);
                //Utility.setNearestSelection(axComponentControl1, DrawingOriginal, p_PointLocationScreen, 10);
                Utility.setSelection(axComponentControl1, DrawingOriginal, p_PointLocationScreen, 10);
                
                axComponentControl1.Refresh();
            }
            catch (Exception err)
            {
                MessageBox.Show("ERROR:" + err.Message);
            }
        }

        private void axComponentControl1_EndTrack(object sender, AxManifold.Interop.IComponentControlEvents_EndTrackEvent e)
        {
            try
            {
                switch (axComponentControl1.MouseMode)
                {
                    case Manifold.Interop.ControlMouseMode.ControlMouseModeGenericBoxCenter:
                        break;
                    case Manifold.Interop.ControlMouseMode.ControlMouseModeGenericPoint:
                        double x, y;
                        x = e.pArgs.GeomLatLon.Box.XMax;
                        y = e.pArgs.GeomLatLon.Box.YMax;
                        DrawPoint(x, y);
                        break;
                    case Manifold.Interop.ControlMouseMode.ControlMouseModeGenericLine:
                        DrawLine(e.pArgs.GeomNative);
                        break;
                    case Manifold.Interop.ControlMouseMode.ControlMouseModeGenericArea:
                        DrawArea(e.pArgs.GeomNative);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                MessageBox.Show("ERROR:" + err.Message);
            }
            finally
            {
                EventHandler_onEditModeChanged(this, new ModeTypeEventArgs(MODETYPE_OPERATION.SelectMode));
            }
        }

        private void DrawPoint(double x, double y)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                Manifold.Interop.Point pt = axComponentControl1.Application.NewPoint(x, y);
                pt = Utility.convertPointCoordinates(axComponentControl1, this.DrawingOriginal, pt);
                Utility.DrawPoint(this.DrawingOriginal, pt);
                Utility.unsetSelection(axComponentControl1, this.DrawingOriginal);
                
                axComponentControl1.Refresh();
            }
            catch (Exception err)
            {
                MessageBox.Show("ERROR:" + err.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void DrawLine(Manifold.Interop.Geom p_objGeomLine)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                Utility.DrawLine(this.DrawingOriginal, p_objGeomLine);
                this.DrawingOriginal.SelectNone();
                axComponentControl1.Refresh();
            }
            catch (Exception err)
            {
                MessageBox.Show("ERROR:" + err.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void DrawArea(Manifold.Interop.Geom p_objGeomArea)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                Utility.DrawArea(this.DrawingOriginal, p_objGeomArea);
                this.DrawingOriginal.SelectNone();
                axComponentControl1.Refresh();
            }
            catch (Exception err)
            {
                MessageBox.Show("ERROR:" + err.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

    }
}
