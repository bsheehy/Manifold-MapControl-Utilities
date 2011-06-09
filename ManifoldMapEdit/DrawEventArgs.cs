using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManifoldMapEdit
{
    public class DrawEventArgs : System.EventArgs
    {
        private Type _typeModified;
        private DrawEventType _DrawEventAction;
        private Int32 _ManifoldID;

        /// <summary>
        /// This is the original GEOM string before it was edited via the 'EditDrawingFactorySQL' class.
        /// This needs set from the 'EditDrawingFactorySQL' class when a GEOM is edited because if editing
        /// an Exclusion GEOM then need the original GEOM to recalculate the Parent containing GEOM.
        /// </summary>
        private string _OriginalManifoldGEOM;
        private string _DrawingName;
        private Manifold.Interop.Drawing _Drawing;
        private GeomMovedDetails _GeomMovedDetails;

        #region Constructors
        public DrawEventArgs(DrawEventType p_DrawEventAction, string p_DrawingName, Int32 p_ManifoldID)
        {
            this._DrawingName = p_DrawingName;
            this._DrawEventAction = p_DrawEventAction;
            this._ManifoldID = p_ManifoldID;
        }

        /// <summary>
        /// This ManifoldInspections.EventArgs.DrawEventArgs constructor is used in the EditDrawingFactorySQL class.
        /// </summary>
        /// <param name="p_DrawEventAction"></param>
        /// <param name="p_Drawing"></param>
        /// <param name="p_ManifoldID"></param>
        /// <param name="p_ManifoldGEOMOriginal"></param>
        public DrawEventArgs(DrawEventType p_DrawEventAction, Manifold.Interop.Drawing p_Drawing, Int32 p_ManifoldID, string p_ManifoldGEOMOriginal)
        {
            this._OriginalManifoldGEOM = p_ManifoldGEOMOriginal;
            this._Drawing = p_Drawing;
            this._DrawEventAction = p_DrawEventAction;
            this._ManifoldID = p_ManifoldID;
        }

        public DrawEventArgs(Type p_TypeModified, DrawEventType p_DrawEventAction, string p_DrawingName, Int32 p_ManifoldID)
        {
            this._DrawingName = p_DrawingName;
            this._typeModified = p_TypeModified;
            this._DrawEventAction = p_DrawEventAction;
            this._ManifoldID = p_ManifoldID;
        }
        #endregion

        public string DrawingName
        {
            get
            {
                return _DrawingName;
            }
        }

        public Type TypeModified
        {
            get
            {
                return _typeModified;
            }
        }

        public DrawEventType DrawEventAction
        {
            get
            {
                return _DrawEventAction;
            }
        }

        public Int32 ManifoldID
        {
            get
            {
                return _ManifoldID;
            }
        }

        public Manifold.Interop.Drawing Drawing
        {
            get
            {
                return _Drawing;
            }
        }


        public string OriginalManifoldGEOM
        {
            get
            {
                return _OriginalManifoldGEOM;
            }
        }

        public GeomMovedDetails GeomMovedDetails
        {
            get
            {
                return _GeomMovedDetails;
            }
            set
            {
                _GeomMovedDetails = value;
            }
        }
    }
}
