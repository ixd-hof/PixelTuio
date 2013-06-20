using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Diagnostics;

using Bespoke.Common.Osc;

namespace Tuio
{
    /// <summary>
    /// Simple, still uncomplete implementation of a TUIO server in C#.
    /// 
    /// Current shortcomings:
    /// Object support missing.
    /// Does not implement frame times.
    /// Only supports external TUIO cursors.
    /// Allways commits all cursors.
    /// 
    /// (c) 2010 by Dominik Schmidt (schmidtd@comp.lancs.ac.uk)
    /// </summary>
    public class TuioServer
    {
        #region constants
        
        private const string _cursorAddressPattern = "/tuio/2Dcur";

        private const string _objectAddressPattern = "/tuio/2Dobj"; 

        #endregion

        #region fields

        private IPEndPoint _ipEndPoint;

        private Dictionary<int, TuioCursor> _cursors;

        private Dictionary<int, TuioObject> _objects;

        private int _currentFrame;

        #endregion

        #region constructors

        /// <summary>
        /// Creates a new server with and endpoint at localhost, port 3333.
        /// </summary>
        public TuioServer() : this("127.0.0.1", 3333) { }

        /// <summary>
        /// Creates a new server.
        /// </summary>
        /// <param name="host">Endpoint host</param>
        /// <param name="port">Endpoint port</param>
        public TuioServer(string host, int port)
        {
            _cursors = new Dictionary<int, TuioCursor>();
            _objects = new Dictionary<int, TuioObject>();
            _ipEndPoint = new IPEndPoint(IPAddress.Parse(host), port);
            _currentFrame = 0;
        }

        #endregion

        #region frame related methods

        /// <summary>
        /// Initialized a new frame and increases the frame counter.
        /// </summary>
        public void InitFrame()
        {
            _currentFrame++;
        }

        /// <summary>
        /// Commits the current frame.
        /// </summary>
        public void CommitFrame()
        {
            GetCursorFrameBundle().Send(_ipEndPoint);
            GetObjectFrameBundle().Send(_ipEndPoint);
        }

        #endregion

        #region cursor related methods
        
        /// <summary>
        /// Adds a TUIO cursor. A new id, not used before, must be provided.
        /// </summary>
        /// <param name="id">New id</param>
        /// <param name="location">Location</param>
        public void AddTuioCursor(int id, PointF location)
        {
            lock(_cursors)
                if(!_cursors.ContainsKey(id))
                    _cursors.Add(id, new TuioCursor(id, location));
        }

        /// <summary>
        /// Updates a TUIO cursor. An id of an existing cursor must be provided.
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="location">Location</param>
        public void UpdateTuioCursor(int id, PointF location)
        {
            TuioCursor cursor;
            if(_cursors.TryGetValue(id, out cursor))
                cursor.Location = location;
        }

        /// <summary>
        /// Deletes a TUIO cursor. An id of an existing cursor must be provided.
        /// </summary>
        /// <param name="id">Id</param>
        public void DeleteTuioCursor(int id)
        {
            lock (_cursors)
                _cursors.Remove(id);
        }

        #endregion


        #region object related methods

        /// <summary>
        /// Adds a TUIO object. A new id, not used before, must be provided.
        /// </summary>
        /// <param name="id">New id</param>
        /// <param name="location">Location</param>
        public void AddTuioObject(int id, long classid, PointF location, float orientation)
        {
            lock (_objects)
                if (!_objects.ContainsKey(id))
                    _objects.Add(id, new TuioObject(id, classid, location, orientation));
        }

        /// <summary>
        /// Updates a TUIO cursor. An id of an existing cursor must be provided.
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="location">Location</param>
        public void UpdateTuioObject(int id, long classid, PointF location, float orientation)
        {
            TuioObject tuioobject;
            if (_objects.TryGetValue(id, out tuioobject))
            {
                tuioobject.Location = location;
                tuioobject.Orientation = orientation;
            }
        }

        /// <summary>
        /// Deletes a TUIO cursor. An id of an existing cursor must be provided.
        /// </summary>
        /// <param name="id">Id</param>
        public void DeleteTuioObject(int id)
        {
            Debug.WriteLine("remove: " + id);
            lock (_objects)
                _objects.Remove(id);
        }

        #endregion


        #region osc message assembly

        private OscBundle GetCursorFrameBundle()
        {
            OscBundle bundle = new OscBundle(_ipEndPoint);

            bundle.Append(GetCursorAliveMessage());
            foreach (OscMessage msg in GetCursorMessages())
                bundle.Append(msg);
            bundle.Append(GetCursorSequenceMessage());

            return bundle;
        }

        private OscBundle GetObjectFrameBundle()
        {
            OscBundle bundle = new OscBundle(_ipEndPoint);

            bundle.Append(GetObjectAliveMessage());
            foreach (OscMessage msg in GetObjectMessages())
                bundle.Append(msg);
            bundle.Append(GetObjectSequenceMessage());

            return bundle;
        }

        private OscMessage GetCursorAliveMessage()
        {
            OscMessage msg = new OscMessage(_ipEndPoint, _cursorAddressPattern);

            msg.Append("alive");
            lock (_cursors)
                foreach (TuioCursor cursor in _cursors.Values)
                    msg.Append((Int32)cursor.Id);

            return msg;
        }

        private OscMessage GetObjectAliveMessage()
        {
            OscMessage msg = new OscMessage(_ipEndPoint, _objectAddressPattern);

            msg.Append("alive");
            lock (_objects)
                foreach (TuioObject tuioobject in _objects.Values)
                    msg.Append((Int32)tuioobject.Id);

            return msg;
        }

        private OscMessage GetCursorSequenceMessage()
        {
            OscMessage msg;
            msg  = new OscMessage(_ipEndPoint, _cursorAddressPattern);

            msg.Append("fseq");
            msg.Append((Int32)_currentFrame);

            return msg;
        }

        private OscMessage GetObjectSequenceMessage()
        {
            OscMessage msg;
            msg = new OscMessage(_ipEndPoint, _objectAddressPattern);

            msg.Append("fseq");
            msg.Append((Int32)_currentFrame);

            return msg;
        }

        private OscMessage GetCursorMessage(TuioCursor cursor)
        {
            /// /tuio/2Dcur set s x y m r
            /// 
            OscMessage msg = new OscMessage(_ipEndPoint, _cursorAddressPattern);

            msg.Append("set");
            msg.Append((Int32)cursor.Id); // s
            msg.Append(cursor.Location.X); // x
            msg.Append(cursor.Location.Y); // y
            msg.Append(0.0f); // m
            msg.Append(0.0f); // r
            msg.Append(0.0f); // ?
            msg.Append(0.0f); // ?

            return msg;
        }

        private OscMessage GetObjectMessage(TuioObject tuioobject)
        {
            /// tuio/2Dobj set s i x y a X Y A m r
            /// (*fullPacket) << (int32)((*tuioObject)->getSessionID()) << (*tuioObject)->getSymbolID() << (*tuioObject)->getX() << (*tuioObject)->getY() << (*tuioObject)->getAngle();
            /// (*fullPacket) << (*tuioObject)->getXSpeed() << (*tuioObject)->getYSpeed() << (*tuioObject)->getRotationSpeed() << (*tuioObject)->getMotionAccel() << (*tuioObject)->getRotationAccel();	
            
            OscMessage msg = new OscMessage(_ipEndPoint, _objectAddressPattern);

            msg.Append("set");
            msg.Append((Int32)tuioobject.Id); // s
            msg.Append((Int32)tuioobject.ClassId); // i
            msg.Append(tuioobject.Location.X); // x
            msg.Append(tuioobject.Location.Y); // y
            msg.Append(tuioobject.Orientation); // a
            msg.Append((float)0.0f); // X
            msg.Append((float)0.0f); // Y
            msg.Append((float)0.0f); // A
            msg.Append((float)0.0f); // m
            msg.Append((float)0.0f); // r

            return msg;
        }

        private IEnumerable<OscMessage> GetCursorMessages()
        {
            List<OscMessage> msgs = new List<OscMessage>();

            lock (_cursors)
                foreach (TuioCursor cursor in _cursors.Values)
                    msgs.Add(GetCursorMessage(cursor));

            return msgs.AsEnumerable();
        }

        private IEnumerable<OscMessage> GetObjectMessages()
        {
            List<OscMessage> msgs = new List<OscMessage>();

            lock (_objects)
                foreach (TuioObject tuioobject in _objects.Values)
                    msgs.Add(GetObjectMessage(tuioobject));

            return msgs.AsEnumerable();
        }

        #endregion
    }
}
