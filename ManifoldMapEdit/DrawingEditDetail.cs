using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManifoldMapEdit
{
    /// <summary>
    /// This class represents the Edit Drawing layer used in the EditDrawingFactory.cs. This layer
    /// provides Edit feature functionality to the ActiveX manifold map control.
    /// </summary>
    public class DrawingEditDetail 
    {
        public DrawingEditDetail()
        {
        }

        public DrawingEditDetail(Manifold.Interop.Point p_objPoint)
        {
            this.X = p_objPoint.X;
            this.Y = p_objPoint.Y;
            this.XCoord = p_objPoint.X.ToString();
            this.YCoord = p_objPoint.Y.ToString();
        }

        #region Point, Line or Area Properties
        private Int32 _ID = -1;
        private double _X;
        private double _Y;
        private string _XCoord;
        private string _YCoord;
        private int _BranchIndex;
        private int _PointIndex;
        private int _BranchMaxIndex;
        private string _DateCreated = "";
        private bool _SnapPoint = false;
        /// <summary>
        /// Snap Types are:
        ///   0 = No snap type e.g. its not a snap node
        ///   1 = From Node 
        ///   2 = To Node
        /// </summary>
        private int _SnapType = 0;

        private bool _IsExclusion = false;

        public string XCoord
        {
            get
            {
                return _XCoord;
            }
            set
            {
                _XCoord = value;
            }
        }

        public string YCoord
        {
            get
            {
                return _YCoord;
            }
            set
            {
                _YCoord = value;
            }
        }

        public double X
        {
            get
            {
                return _X;
            }
            set
            {
                _X = value;
            }
        }

        public double Y
        {
            get
            {
                return _Y;
            }
            set
            {
                _Y = value;
            }
        }

        public Int32 ID
        {
            get
            {
                return _ID;
            }
            set
            {
                _ID = value;
            }
        }

        public bool SnapPoint
        {
            get
            {
                return _SnapPoint;
            }
            set
            {
                _SnapPoint = value;
            }
        }

        public string DateCreated
        {
            get
            {
                return _DateCreated;
            }
            set
            {
                _DateCreated = value;
            }
        }

        public int BranchIndex
        {
            get
            {
                return _BranchIndex;
            }
            set
            {
                _BranchIndex = value;
            }
        }

        public int PointIndex
        {
            get
            {
                return _PointIndex;
            }
            set
            {
                _PointIndex = value;
            }
        }

        public int BranchMaxIndex
        {
            get
            {
                return _BranchMaxIndex;
            }
            set
            {
                _BranchMaxIndex = value;
            }
        }

        public int SnapType
        {
            get
            {
                return _SnapType;
            }
            set
            {
                _SnapType = value;
            }
        }

        #endregion
    }
}
