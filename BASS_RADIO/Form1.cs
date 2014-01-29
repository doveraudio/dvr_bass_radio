using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using utility;

namespace BASS_RADIO
{
    public partial class Form1 : Form
    {
        private BassRadio myRadio;
        private WebBassLibrary mySites;

        public Task<bool> Play()
        {
            return Task.Factory.StartNew<bool>(myRadio.Play);
        }

        public Task<string> GetCurrentStream()
        {
            return Task.Factory.StartNew<string>(_current_stream);
        }

        public string _current_stream()
        {
            myRadio.URL = mySites.current().URL;
            return mySites.current().URL;
        }

        public Form1()
        {
            InitializeComponent();
            myRadio = new BassRadio();
            mySites = new WebBassLibrary();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            myRadio.Init();
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (mySites.isEmpty())
            {
                WebBassItem track = new WebBassItem();
                track.URL = myRadio.defaultUrl();
                track.Title = "DNBHeaven";
                mySites.add(track);
                GetCurrentStream();
                guiAddTrack(track);
            }
            else
            {
                GetCurrentStream();
            }
            myRadio.Stop();
            GetCurrentStream();
            Play();

            txtTrackname.Text = myRadio.Title;
            txtArtist.Text = myRadio.Artist;
        }

        private void guiAddTrack(WebBassItem track)
        {
            ListViewItem myItem = new ListViewItem();
            myItem.Text = track.Title;
            myItem.Tag = track.URL;
            myItem.Name = track.Title;

            listViewPlaylists.Items.Add(myItem);
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            WebBassItem track = new WebBassItem();
            track.URL = txtUrl.Text;
            track.Title = txtTitle.Text;
            mySites.add(track);
            guiAddTrack(track);
        }

        private void txtTitle_TextChanged(object sender, EventArgs e)
        {
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            myRadio.Stop();
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            mySites.previous();
            GetCurrentStream();
            txtTrackname.Text = myRadio.Title;
            txtArtist.Text = myRadio.Artist;
            txtTitle.Text = mySites.current().Title;
            txtUrl.Text = mySites.current().URL;
            Play();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            mySites.next();
            GetCurrentStream();
            txtTrackname.Text = myRadio.Title;
            txtArtist.Text = myRadio.Artist;
            txtTitle.Text = mySites.current().Title;
            txtUrl.Text = mySites.current().URL;
            Play();
        }

        private void btnsave_Click(object sender, EventArgs e)
        {
            mySites.save();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            mySites.load();
            foreach (WebBassItem track in mySites.dump())
            {
                guiAddTrack(track);
            }
        }

        private void listViewPlaylists_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtArtist.Text = listViewPlaylists.SelectedItems[0].Text;
            txtTitle.Text = listViewPlaylists.SelectedItems[0].Name;
            txtUrl.Text = listViewPlaylists.SelectedItems[0].Tag.ToString();
        }

        private void listViewPlaylists_ItemActivate(object sender, EventArgs e)
        {
            myRadio.URL = listViewPlaylists.SelectedItems[0].Tag.ToString();
            Play();
            txtTrackname.Text = myRadio.Title;
            txtArtist.Text = myRadio.Artist;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
        }
    }
}