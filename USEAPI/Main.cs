using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace USEAPI
{
    public partial class Main : Form
    {
        private Setting? setting;
        private readonly AppSettingsStore settingsStore = new AppSettingsStore();
        private readonly PapagoClient papagoClient = new PapagoClient();
        public Main()
        {
            InitializeComponent();
        }

        public static string HomeURL = AppSettings.DefaultHomeUrl;       //기본 홈 설정, Setting.cs에서 이용
        private TranslationDirection translationDirection = TranslationDirection.KoreanToEnglish;

        #region Form1 로드시 기본 세팅
        private void Form1_Load(object sender, EventArgs e)
        {
            HomeURL = settingsStore.Load().HomeUrl;
            
            Web_URL.Text = HomeURL;
            NavigateWebBrowser(Web_Search, Web_URL.Text);
            this.Invalidate();

            //before 기본텍스트 입력
            before.Text = "입력하세요";
        }
        #endregion

        #region 파일열기 탭
        private void FindText_Click(object sender, EventArgs e)
        {
            openFileDialog2.Filter = "텍스트파일(*.txt)|*.txt|C파일(*.c)|*.c|C++파일(*.cpp)|*.cpp|C#파일(*.cs)|*.cs";
            if (openFileDialog2.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            FindText_URL.Text = openFileDialog2.FileName;
            try
            {
                richTextBox1.Text = File.ReadAllText(openFileDialog2.FileName, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                ErrorLog.Write(ex);
                richTextBox1.Text = string.Format("읽을 수 없는 파일입니다.\r\n{0}", ex.Message);
            }
        }
        private void FindFile_Click(object sender, EventArgs e)
        {
            FindFile_URL.Clear();

            if (openFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            FindFile_URL.Text = openFileDialog1.FileName;
            NavigateWebBrowser(web, openFileDialog1.FileName);
        }
        #endregion
        
        #region 마우스 클릭 이벤트
        //마우스 우클릭
        private void richTextBox1_MouseDown(object sender, MouseEventArgs e)
        {
            //마우스 우클릭
            if (e.Button == MouseButtons.Right)
            {
                before.Text = richTextBox1.SelectedText;
            }
            //마우스 휠클릭(한글 클릭시 오류)
            if (e.Button == MouseButtons.Middle)
            {
                SearchSelectedText(richTextBox1.SelectedText);
            }
        }
        #endregion

        #region 웹검색 버튼
        private void GoBack_Click(object sender, EventArgs e)
        {
            if (Web_Search.CanGoBack)
            {
                Web_Search.GoBack();
            }
        }
        private void GoForward_Click(object sender, EventArgs e)
        {
            if (Web_Search.CanGoForward)
            {
                Web_Search.GoForward();
            }
        }
        private void GoHome_Click(object sender, EventArgs e)
        {
            Web_URL.Text = HomeURL;
            Web_URL_Btn_Click(sender, e);
        }
        private void Web_URL_Btn_Click(object sender, EventArgs e)
        {
            NavigateWebBrowser(Web_Search, Web_URL.Text);
        }
        private void Web_URL_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Web_URL_Btn_Click(sender, e);
            }
        }
        private void Web_Search_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            if (Web_Search.Url != null)
            {
                Web_URL.Text = Web_Search.Url.AbsoluteUri;
            }
        }
        #endregion

        #region 번역기 버튼
        private void Clear_before_Btn_Click(object sender, EventArgs e)
        {
            before.Clear();
        }

        private async void Translate_Btn_Click(object sender, EventArgs e)
        {
            await Papago_Api();
        }
        private void Change_Btn_Click(object sender, EventArgs e)
        {
            if (translationDirection == TranslationDirection.KoreanToEnglish)
            {
                translationDirection = TranslationDirection.EnglishToKorean;
                Change_Btn.Text = "영어 -> 한글";
            }
            else
            {
                translationDirection = TranslationDirection.KoreanToEnglish;
                Change_Btn.Text = "한글 -> 영어";
            }
        }
        private void before_KeyDown(object sender, KeyEventArgs e)
        {
            //번역 텍스트칸에서 엔터 입력시 번역 작동
            if (e.KeyCode == Keys.Enter)
            {
                Translate_Btn_Click(sender, e);
            }
        }
        #endregion

        #region 파파고 번역
        //파파고
        private async Task Papago_Api()
        {
            try
            {
                Translate_Btn.Enabled = false;
                after.Text = await papagoClient.TranslateAsync(before.Text, translationDirection);
            }
            catch(Exception ex)
            {
                ErrorLog.Write(ex);
                MessageBox.Show(ex.Message, "번역 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Translate_Btn.Enabled = true;
            }
        }
        #endregion
        
        #region 환경설정 - Setting
        private void 검색ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (setting == null)
            {
                setting = new Setting();
                setting.Owner = this;
                setting.Changed += SettingApply;  // 컨트롤 등의 변경
                setting.CloseSetting += SettingClose; // 닫음
                setting.Show();
            }
            else
            {
                setting.Focus();
            }
        }

        public void SettingApply(object? sender, EventArgs e)
        {
            if (setting == null)
            {
                return;
            }

            HomeURL = setting.SetURL;     // pathSetting Form에서  Rootpath 정보를 가져와서 ~
            Web_URL.Text = HomeURL;
        }

        public void SettingClose(object? sender, EventArgs e)
        {
            //모달리스 종료 처리
            setting?.Dispose();
            setting = null;
        }

        #endregion

        private void SearchSelectedText(string selectedText)
        {
            if (string.IsNullOrWhiteSpace(selectedText))
            {
                return;
            }

            Web_URL.Text = "https://search.naver.com/search.naver?ie=UTF-8&query=" + Uri.EscapeDataString(selectedText);
            Web_URL_Btn_Click(this, EventArgs.Empty);
        }

        private void NavigateWebBrowser(WebBrowser browser, string address)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(address))
                {
                    MessageBox.Show("이동할 주소를 입력하세요.", "주소 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Uri? uri;
                if (File.Exists(address))
                {
                    uri = new Uri(Path.GetFullPath(address));
                }
                else if (!Uri.TryCreate(address, UriKind.Absolute, out uri) ||
                    (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeFile))
                {
                    MessageBox.Show("http, https 또는 파일 경로만 열 수 있습니다.", "주소 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                browser.Navigate(uri);
            }
            catch (Exception ex)
            {
                ErrorLog.Write(ex);
                MessageBox.Show(ex.Message, "이동 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

/*todo
 * 수정됨 - 번역 영어>한글 기능 추가
 * 수정됨 - 오류 수정 try/catch 추가함
 * 수정됨 - 웹페이지 기본 사이트 설정
 * 수정됨 - 웹피이지 이동시 표시되는 url경로 수정
 * 수정됨 - 홈페이지 url 외부 입출력
 * 수정됨 - Main폼에서 Setting 폼 연동
 * 
 */
