﻿using System;
using System.Diagnostics;
using iSpyApplication.Controls;

namespace iSpyApplication.Sources.Video
{
    internal class VideoBase: FFmpegBase
    {
        private readonly CameraWindow _cw;
        public string Tokenise(string sourcestring)
        {
            if (_cw == null)
                return sourcestring;

            var vss = _cw.Camobject.settings.videosourcestring;
            if (vss.IndexOf("[TOKEN]", StringComparison.Ordinal) != -1)
            {
                var t = new Tokeniser(vss, _cw.Camobject.settings.login, _cw.Camobject.settings.password, _cw.Camobject.settings.tokenconfig.tokenpath, _cw.Camobject.settings.tokenconfig.tokenport, _cw.Camobject.settings.tokenconfig.tokenpost);
                t.Populate();
                var url = vss.Replace("[TOKEN]", t.Token);
                //if (t.ReconnectInterval > 0)
                //    _source.settings.reconnectinterval = t.ReconnectInterval;
                return url;
            }
            return sourcestring;
        }

        public VideoBase(CameraWindow cw):base("FFMPEG")
        {
            _cw = cw;
        }
        
        private DateTime _nextFrameTarget = DateTime.MinValue;
        public bool EmitFrame
        {
            get
            {
                if (_cw == null)
                    return true;
                var d = Helper.Now;
                if (d < _nextFrameTarget)
                {
                    return false;
                }

                double dMin = FrameInterval;
                _nextFrameTarget = _nextFrameTarget.AddMilliseconds(dMin);

                if (_nextFrameTarget < d)
                {
                    _nextFrameTarget = d;
                }

                return true;
            }
        }

        public int FrameInterval
        {
            get
            {
                if (_cw == null)
                    return 200;

                decimal r = _cw.Camobject.settings.maxframerate;
                if (_cw.Recording)
                    r = _cw.Camobject.settings.maxframeraterecord;

                r = Math.Max(0.01m,Math.Min(r, MainForm.ThrottleFramerate));
                return Convert.ToInt32(1000m/r);

            }
        }

    }
}
