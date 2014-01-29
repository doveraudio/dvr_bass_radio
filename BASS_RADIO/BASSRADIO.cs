using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Tags;

namespace BASS_RADIO
{
    public class BassRadio
    {
        private string _url;
        public int stream;
        private List<int> streams;
        private bool _started;
        private TAG_INFO _tags;
        private bool playing;

        public string URL
        {
            get { return _url; }
            set { _url = value; }
        }

        public bool Started
        {
            get { return _started; }
        }

        public bool Download()
        {
            if (URL.EndsWith(".mp3"))
            {
                using (System.Net.WebClient Client = new System.Net.WebClient())
                {
                    Client.DownloadFile(URL, Title + ".mp3");
                }
                return true;
            }
            else { return false; }
        }

        public bool Init()
        {
            if (Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero))
            {
                // it's good
                Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_NET_PLAYLIST, 1);
                _tags = new TAG_INFO();
                _started = true;
                streams = new List<int>();
                return true;
            }
            else
            {
                // it's not
                _started = false;
                return false;
            }
        }

        public int Channel(string url)
        {
            if (_url != "")
            {
                URL = _url;
                stream = Bass.BASS_StreamCreateURL(_url, 0, BASSFlag.BASS_STREAM_STATUS, null, IntPtr.Zero);
                streams.Add(stream);
                return stream;
            }
            else
            {
                return 0;
            }
        }

        public int next(string url)
        {
            Stop();
            Channel(url);
            Play();
            return stream;
        }

        public int previous(string url)
        {
            Stop();
            Channel(url);
            Play();
            return stream;
        }

        public bool Play()
        {
            if (Started)
            {
                if (playing) { Stop(); }
                Channel(_url);

                Bass.BASS_ChannelPlay(stream, false);
                playing = true;
                return true;
            }
            else
            {
                playing = false;
                return false;
            }
        }

        public bool StopAll()
        {
            if (Started)
            {
                playing = false;
                foreach (int stream in streams)
                {
                    Bass.BASS_ChannelStop(stream);
                    Bass.BASS_StreamFree(stream);
                }
                return true;
            }
            else { playing = false; return false; }
        }

        public bool Stop()
        {
            if (Started)
            {
                playing = false;

                Bass.BASS_ChannelStop(this.stream);
                Bass.BASS_StreamFree(this.stream);
                return true;
            }
            else { playing = false; return false; }
        }

        public string defaultUrl()
        {
            string url = "http://dnbheaven.com/96kbps.m3u";
            return url;
        }

        public TAG_INFO Tags
        {
            get
            {
                refreshTags();
                return _tags;
            }
        }

        public string Artist { get { refreshTags(); return _tags.artist; } set { _tags.artist = value; } }

        public string Title { get { refreshTags(); return _tags.title; } set { _tags.title = value; } }

        private void refreshTags()
        {
            _tags = new TAG_INFO(URL);
            BassTags.BASS_TAG_GetFromURL(stream, _tags);
        }
    }
}

namespace utility
{
    public class WebBassItem
    {
        private string _url;
        private string _title;
        private int _stream;

        public string URL { get { return _url; } set { _url = value; } }

        public int Stream { get { return _stream; } set { _stream = value; } }

        public string Title { get { return _title; } set { _title = value; } }
    }

    public class WebBassLibrary
    {
        private WebPlaylist MasterList;
        private DataManager DM;
        private int _current;

        public WebBassLibrary()
        {
            this._current = 0;
            this.MasterList = new WebPlaylist();
            this.MasterList.URLs = new List<WebBassItem>();
            this.DM = new DataManager();
        }

        public WebBassItem current()
        {
            if (_current < 0) { _current = MasterList.URLs.Count - 1; }
            if (_current > MasterList.URLs.Count - 1) { _current = 0; }
            return MasterList.URLs.ElementAt(_current);
        }

        public WebBassItem findTitle(string title)
        {
            WebBassItem temp = new WebBassItem();
            temp.URL = "nsi";
            temp.Title = "nsi";
            temp.Stream = 0;
            Predicate<WebBassItem> TitleFinder = (WebBassItem item) => { return item.Title == title; };
            temp = MasterList.URLs.Find(TitleFinder);
            return temp;
        }

        public WebBassItem findURL(string url)
        {
            WebBassItem temp = new WebBassItem();
            temp.URL = "nsi";
            temp.Title = "nsi";
            temp.Stream = 0;
            Predicate<WebBassItem> URLFinder = (WebBassItem item) => { return item.URL == url; };
            temp = MasterList.URLs.Find(URLFinder);
            return temp;
        }

        public WebBassItem next()
        {
            _current += 1; return current();
        }

        public WebBassItem previous()
        {
            _current -= 1; return current();
        }

        public void add(WebBassItem NewItem)
        {
            MasterList.URLs.Add(NewItem);
        }

        public void add(List<WebBassItem> NewList)
        {
            MasterList.URLs.AddRange(NewList);
        }

        public int channelStream()
        {
            return this.current().Stream;
        }

        public bool delete(WebBassItem OldItem)
        {
            return MasterList.URLs.Remove(OldItem);
        }

        public bool save()
        {
            return DM.SavePlaylist(MasterList);
        }

        public bool load()
        {
            this.MasterList = DM.loadPlaylist("default.xml");
            return true;
        }

        public List<WebBassItem> dump()
        {
            return MasterList.URLs;
        }

        public bool isEmpty()
        {
            if (MasterList.URLs.Count > 0) { return false; } else { return true; }
        }
    }

    public class WebPlaylist
    {
        private List<WebBassItem> _urls;

        public List<WebBassItem> URLs
        { get { return _urls; } set { _urls = value; } }
    }

    public class DataManager
    {
        private XmlSerializer xmlOut = new XmlSerializer(typeof(WebPlaylist));

        public bool SavePlaylist(WebPlaylist playlist)
        {
            StreamWriter sw = new StreamWriter("DefaultPlaylist.xml");
            xmlOut.Serialize(sw, playlist);
            return true;
        }

        public WebPlaylist loadPlaylist(string filename)
        {
            WebPlaylist newList;
            StreamReader sr = new StreamReader("DefaultPlaylist.xml");
            newList = (WebPlaylist)xmlOut.Deserialize(sr);
            return newList;
        }
    }
}