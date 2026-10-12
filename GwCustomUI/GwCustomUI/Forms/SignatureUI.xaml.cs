using GwCustomUI.Classes;
using GwCustomUI.Signatures;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.ApplicationServices;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;
using static GwCustomUI.Classes.Constants;
using System.Reflection.Metadata;
using HtmlAgilityPack;
using System.Reflection;
using System.Windows.Threading;

namespace GwCustomUI {
    /// <summary>
    /// Interaktionslogik für SignatureUI.xaml
    /// </summary>
    public partial class SignatureUI : Window {
        public SignatureUI(SignatureRepository repo, SignatureTransport transport) {
            InitializeComponent();
            _repository = repo;
            _transport = transport;

            // Breite an Inhalt anpassen
            this.SizeToContent = SizeToContent.Width;
            this.ShowInTaskbar = true;
            this.Topmost = false;

            // Benutzer und vorherige Auswahl ermitteln
            _user = _repository.GetUserByLogin(CommonUtil.GetConfig().LastUser) ?? _repository.GetAllUsers()[0] ?? throw new Exception("no user");
            _useroptions = CommonUtil.GetConfig().GetUserOptions(_user.Login);
            _layout = _repository.GetLayoutById(_useroptions.Layout) ?? _user.GetLayouts()[0] ?? throw new Exception("no layout");
            _signature = _repository.GetSignatureByIdAndUser(_useroptions.Signature, _user) ?? _layout.GetSignatures(_user)[0] ?? throw new Exception("no signature");

            // Vorschautimer initialisieren
            _previewTimer = new DispatcherTimer {
                Interval = TimeSpan.FromMilliseconds(100)
            };
        }

        private readonly DispatcherTimer _previewTimer;

        private readonly SignatureRepository _repository;

        private readonly SignatureTransport _transport;

        private UserOptions _useroptions;

        private SignatureUser _user;

        private SignatureLayout _layout;

        private SignatureItem _signature;

        private async void Window_Loaded(object sender, RoutedEventArgs e) {
            // Abmessungen korrigieren
            this.MinWidth = this.ActualWidth;
            this.MaxWidth = this.ActualHeight;
            ResizeHeight();

            // Benutzer-Sektion
            Title = $"{APPNAME} - {MultiLanguage.GetText("signaturelayout")}";
            LblUser.FontWeight = FontWeights.Bold;
            LblUser.Content = MultiLanguage.GetText("user");

            // Layouts-Sektion
            LblLayout.FontWeight = FontWeights.Bold;
            LblLayout.Content = MultiLanguage.GetText("layout");
            CbInfo.Content = MultiLanguage.GetText("infographic");

            // Signaturen-Sektion
            LblSignature.FontWeight = FontWeights.Bold;
            LblSignature.Content = MultiLanguage.GetText("signature");
            CbMobile.Content = MultiLanguage.GetText("mobilenumber");

            // Buttons-Sektion
            TxtDms.Text = MultiLanguage.GetText("senddms");
            BtnDms.IsEnabled = CommonUtil.GetConfig().DmsFilingEnabled;
            TxtSend.Text = MultiLanguage.GetText("send");
            BtnSend.IsEnabled = true;
            BtnSend.IsDefault = true;
            BtnSend.Focus();

            // WebView mit geteilter Environment initialisieren (verhindert RPC_E_DISCONNECTED abgestorbener Browserprozesse)
            bool previewAvailable = true;
            try {
                CoreWebView2Environment env = await WebViewEnvironment.GetAsync();
                await WvPreview.EnsureCoreWebView2Async(env);

                // Dialog wurde während der Initialisierung geschlossen
                if (!IsLoaded) {
                    return;
                }

                WvPreview.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
                WvPreview.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                WvPreview.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
                WvPreview.CoreWebView2.Settings.AreDevToolsEnabled = false;
                WvPreview.CoreWebView2.Settings.AreHostObjectsAllowed = false;
                WvPreview.CoreWebView2.Settings.IsBuiltInErrorPageEnabled = false;
                WvPreview.CoreWebView2.Settings.IsGeneralAutofillEnabled = false;
                WvPreview.CoreWebView2.Settings.IsPasswordAutosaveEnabled = false;
                WvPreview.CoreWebView2.Settings.IsPinchZoomEnabled = false;
                WvPreview.CoreWebView2.Settings.IsScriptEnabled = false;
                WvPreview.CoreWebView2.Settings.IsStatusBarEnabled = false;
                WvPreview.CoreWebView2.Settings.IsSwipeNavigationEnabled = false;
                WvPreview.CoreWebView2.Settings.IsWebMessageEnabled = false;
                WvPreview.CoreWebView2.Settings.IsZoomControlEnabled = false;
                WvPreview.CoreWebView2.NavigationStarting += CancelNavigation;
                WvPreview.CoreWebView2.NewWindowRequested += PreventNewWindow;
            } catch (Exception ex) {
                // Vorschau nicht verfügbar, Dialog bleibt funktionsfähig
                previewAvailable = false;
                CommonUtil.GlobalErrorHandler(ex, true);
            }

            // Vorschau-Timer initialisieren
            _previewTimer.Tick += (s, e) => {
                _previewTimer.Stop();
                Preview();
            };

            // Benutzer befüllen (löst Befüllung von Layouts und Signaturen mit aus)
            CmbUser.ItemsSource = _repository.GetAllUsers();
            CmbUser.DisplayMemberPath = "Name";
            CmbUser.SelectedValuePath = "Login";
            CmbUser.SelectedValue = _user.Login;

            // Vorschauerstellung mittels Timer
            if (previewAvailable) {
                TriggerPreview();
            }
        }

        private void Window_DpiChanged(object sender, System.Windows.DpiChangedEventArgs e) {
            ResizeHeight();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e) {
            ResizeHeight();
        }

        private void ResizeHeight() {
            double h = GetCurrentScreenHeight(this);
            if (h < this.ActualHeight) {
                this.Height = h;
            }
        }

        private static double GetCurrentScreenHeight(Window window) {
            var screen = Screen.FromPoint(System.Windows.Forms.Cursor.Position);
            var dpiScale = VisualTreeHelper.GetDpi(window);

            return screen.WorkingArea.Height / dpiScale.DpiScaleY;
        }

        /// <summary>
        /// Navigation in der E-Mailvorschau unterbinden
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void CancelNavigation(object? sender, CoreWebView2NavigationStartingEventArgs args) {
            args.Cancel = !args.Uri.StartsWith("file://", StringComparison.CurrentCultureIgnoreCase) && !args.Uri.StartsWith("data:text/html", StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// Neues Fenster aus der E-Mailvorschau heraus unterbinden
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void PreventNewWindow(object? sender, CoreWebView2NewWindowRequestedEventArgs args) {
            args.Handled = true;
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e) {
            _transport.Storage = false;
            SendMail();
        }

        private void BtnDms_Click(object sender, RoutedEventArgs e) {
            _transport.Storage = true;
            SendMail();
        }

        private void CmbUser_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (CmbUser.SelectedIndex > -1) {
                // Ausgewähltes Objekt ermitteln
                if (CmbUser.SelectedItem is SignatureUser user) {
                    _user = user;
                } else {
                    throw new Exception("no user");
                }

                // Benutzer aktualisieren und Einstellungen aus Benutzerkonfiguration aktualisieren
                _useroptions = CommonUtil.GetConfig().GetUserOptions(_user.Login);
                _layout = _repository.GetLayoutById(_useroptions.Layout) ?? _user.GetLayouts()[0] ?? throw new Exception("no layout");
                _signature = _repository.GetSignatureByIdAndUser(_useroptions.Signature, _user) ?? _layout.GetSignatures(_user)[0] ?? throw new Exception("no signature");

                // Layouts laden und vorbelegen
                LbLayout.ItemsSource = _user.GetLayouts();
                LbLayout.DisplayMemberPath = "Name";
                LbLayout.SelectedValuePath = "Id";
                LbLayout.SelectedItem = null; // Änderung sicherstellen!
                if (LbLayout.Items.Contains(_layout)) {
                    LbLayout.SelectedItem = _layout;
                } else {
                    LbLayout.SelectedIndex = 0;
                }

                TriggerPreview();
            }
        }

        private void LbLayout_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            // Ausgewähltes Objekt ermitteln
            if (LbLayout.SelectedIndex > -1) {
                if (LbLayout.SelectedItem is SignatureLayout layout) {
                    _layout = layout;
                } else {
                    throw new Exception("no layout");
                }

                // Signaturen laden und vorbelegen
                LbSignature.ItemsSource = _layout.GetSignatures(_user);
                LbSignature.DisplayMemberPath = "Name";
                LbSignature.SelectedValuePath = "Id";
                LbSignature.SelectedItem = null; // Änderung sicherstellen!
                if (LbSignature.Items.Contains(_signature)) {
                    LbSignature.SelectedItem = _signature;
                } else {
                    LbSignature.SelectedIndex = 0;
                }

                TriggerPreview();
            }
        }

        private void LbSignature_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (LbSignature.SelectedIndex > -1) {
                // Ausgewähltes Objekt ermitteln
                if (LbSignature.SelectedItem is GwCustomUI.Signatures.SignatureItem signature) {
                    _signature = signature;
                } else {
                    throw new Exception("no signature");
                }

                // Infocheckbox aktualisieren, Signatur muss Platzhaltervariable und Layout Infoinhalt aufweisen
                if (_signature.HasInfo && _layout.HasInfo(_signature.Language)) {
                    CbInfo.IsChecked = _useroptions != null && _useroptions.Info;
                    CbInfo.IsEnabled = true;
                } else {
                    CbInfo.IsChecked = false;
                    CbInfo.IsEnabled = false;
                }

                // Mobilecheckbox aktualisieren, Signatur muss Platzhaltervariable mit vorbefülltem Inhalt aufweisen
                if (_signature.HasMobile) {
                    CbMobile.IsChecked = _useroptions != null && _useroptions.Mobile;
                    CbMobile.IsEnabled = true;
                } else {
                    CbMobile.IsChecked = false;
                    CbMobile.IsEnabled = false;
                }

                TriggerPreview();
            }
        }

        private void CbInfo_Click(object sender, RoutedEventArgs e) {
            TriggerPreview();
        }

        private void CbMobile_Click(object sender, RoutedEventArgs e) {
            TriggerPreview();
        }

        private void TriggerPreview() {
            if (WvPreview.CoreWebView2 == null) {
                return;
            }
            _previewTimer.Stop();
            _previewTimer.Start();
        }

        private void Preview() {
            // Signaturinhalt ermitteln
            string signature = _signature.GetContent(CbInfo.IsChecked ?? false, CbMobile.IsChecked ?? false, _layout.GetInfo(_signature.Language));

            // Html Dokument zusammenbauen
            HtmlAgilityPack.HtmlDocument? document = new();
            document.LoadHtml(_transport.Original);

            // Rootcontainer der Nachricht finden (Verläufe sind separat weiter unten auf der selben Ebene)
            HtmlNode? divRoot = document.DocumentNode.SelectSingleNode($".//div[contains(@class,'{CLASS_GW_MAIN}') and contains(@id, '{ID_GW_SECTION}')]");
            if (divRoot != null) {
                // Contentcontainer innerhalb des Layoutcontainer lokalisieren und vorhandenen Inhalt zwischenspeichern
                string content = string.Empty;
                HtmlNode? divContent = divRoot.SelectSingleNode($"./div[@class=\"{CLASS_LAYOUT}\"]/div[@class=\"{CLASS_CONTENT}\"]");
                if (divContent != null) {
                    content = CommonUtil.FormatWordsBold(divContent.InnerHtml);
                }

                // Layoutcontainer lokalisieren und mit Layoutcode befüllen
                HtmlNode? divLayout = divRoot.SelectSingleNode($"./div[@class=\"{CLASS_LAYOUT}\"]");
                if (divLayout != null) {
                    divLayout.InnerHtml = _layout.Content;

                    // Contentcontainer erneut innerhalb des überschriebenen Layoutcontainer lokalisieren und gegebenenfalls erstellen
                    HtmlNodeCollection? nodes = divLayout.SelectNodes($".//div[@class=\"{CLASS_CONTENT}\"]");
                    if (nodes != null && nodes.Count > 0) {
                        divContent = nodes.First();
                    } else {
                        divContent = HtmlNode.CreateNode($"<div class=\"{CLASS_CONTENT}\"></div>");
                        divContent = divLayout.AppendChild(divContent);
                    }
                    if (divContent != null) {
                        // Zwischengespeicherten Content wieder einfügen
                        divContent.InnerHtml = content;

                        // Signaturcontainer innerhalb des Contentcontainer lokalisieren (aus RestructureHtmlFromView sicher vorhanden) und mit Signaturcode befüllen
                        HtmlNode? divSignature = divContent.SelectSingleNode($".//div[@class=\"{CLASS_SIGNATURE}\"]");
                        if (divSignature != null && !string.IsNullOrEmpty(signature)) {
                            divSignature.InnerHtml = CommonUtil.FormatWordsBold(signature);
                        }
                    }
                }
            }

            // Rückgabe des vollständigen Htmlcode
            _transport.Modified = document.DocumentNode.OuterHtml;

            // Vorschaudatei aus Sicherheitsgründen notwendig, NavigateToString ist größenbeschränkt und kann keine lokalen Bildverweise anzeigen
            string previewfile = CommonUtil.CreateTempFile(".htm");
            CommonUtil.UpdateTempFile(previewfile, _transport.Modified);
            string uri = new Uri(previewfile, UriKind.Absolute).AbsoluteUri;
            WvPreview.CoreWebView2.Navigate(uri);
        }

        private void SendMail() {
            // Leere Vorschau abfangen
            if (string.IsNullOrEmpty(_transport.Modified)) {
                DialogResult = false; // Signaturfenster offen lassen
                throw new Exception(MultiLanguage.GetText("nopreviewcontent"));
            }

            // Verwendete Einstellungen speichern
            ConfigModel config = CommonUtil.GetConfig();
            config.LastUser = CmbUser.SelectedValue.ToString() ?? string.Empty;
            UserOptions options = config.GetUserOptions(config.LastUser);
            options.Layout = int.Parse(LbLayout.SelectedValue.ToString() ?? "-1");
            options.Signature = int.Parse(LbSignature.SelectedValue.ToString() ?? "-1");
            options.Info = CbInfo.IsChecked ?? false;
            options.Mobile = CbMobile.IsChecked ?? false;
            config.Save();

            DialogResult = true;
        }
    }
}
