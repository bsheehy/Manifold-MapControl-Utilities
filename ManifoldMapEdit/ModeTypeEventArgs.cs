using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManifoldMapEdit
{
    public class ModeTypeEventArgs : System.EventArgs
    {
        #region Variables
        public MODETYPE_OPERATION MODETYPE_OPERATION;

        #endregion

        #region Constructors

        public ModeTypeEventArgs(MODETYPE_OPERATION modeTypeOperation)
        {
            this.MODETYPE_OPERATION = modeTypeOperation;
        }

        #endregion
    }
}
