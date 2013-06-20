using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Tuio
{
    /// <summary>
    /// TUIO cursor.
    /// 
    /// (c) 2010 by Dominik Schmidt (schmidtd@comp.lancs.ac.uk)
    /// </summary>
    public class TuioObject
    {
        #region properties

        /// s        sessionID, temporary ob ject ID, int32 
        /// i        classID, fiducial ID number, int32 
        /// x, y, z        position, float32, range 0...1 
        /// a, b, c        angle, float32, range 0..2PI 
        /// X, Y ,Z        movement vector (motion speed &amp; direction), float32 
        /// A, B, C        rotation vector (rotation speed &amp; direction), float32 
        /// m        motion acceleration, float32 
        /// r        rotation acceleration, float32 
        /// P        free parameter, type defined by OSC packet header
        /// 
        /// tuio/2Dobj set s i x y a X Y A m r

        public int Id { get; private set; } // s

        public long ClassId { get; private set; } // i

        public PointF Location { get; set; } // x, y

        public float Orientation { get; set; } // a

        public PointF Speed { get; set; }

        public float MotionAcceleration { get; set; } // m

        #endregion

        #region constructors

        public TuioObject(int id, long classid, PointF location, float orientation)
        {
            Id = id;
            ClassId = classid;
            Location = location;
            Orientation = orientation;
        }

        #endregion

    }
}
