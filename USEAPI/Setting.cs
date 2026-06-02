using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace USEAPI
{
    public partial class Setting : Form
    {
        public event EventHandler Changed = null;
        public event EventHandler CloseSetting = null;
        private readonly AppSettingsStore settingsStore = new AppSettingsStore();
        private bool closeNotified;

        public string SetURL
        {
            get { return SetHomeUrl.Text; }
        }

        public Setting()
        {
            InitializeComponent();

            SetText_label2();
            SetHomeUrl.Text = Main.HomeURL;
            //창 우측상단 x키로 닫는 경우 정상종료시키기
            this.FormClosed += Setting_FormClosed;
        }

        private void SetText_label2()
        {
            label2.Text = "설정된 주소 : " + Main.HomeURL;
        }

        #region Ok/Cancel 버튼
        private void OK_Btn_Click(object sender, EventArgs e)
        {
            var settings = new AppSettings { HomeUrl = SetHomeUrl.Text };
            string message;
            if (!settingsStore.Save(settings, out message))
            {
                MessageBox.Show(string.Format("설정을 저장할 수 없습니다.\r\n{0}", message), "설정 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SetHomeUrl.Text = settings.HomeUrl;

            if (Changed != null)
                Changed(this, new EventArgs());

            this.Close();
        }

        private void Cancel_Btn_Click(object sender, EventArgs e)
        {
            this.Close();

        }

        private void Setting_FormClosed(object sender, FormClosedEventArgs e)
        {
            NotifyClosed();
        }

        private void NotifyClosed()
        {
            if (closeNotified)
            {
                return;
            }

            closeNotified = true;
            if (CloseSetting != null)
                CloseSetting(this, new EventArgs());
        }
        #endregion

        
        
            
    }
}
