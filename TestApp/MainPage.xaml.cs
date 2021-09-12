using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Media.Editing;
using Windows.Media.Effects;
using WinrtFilter;
using Windows.Media.MediaProperties;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TestApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        MediaEncodingProfile m_profile;
        VideoEncodingProperties m_vProperty;
        MediaComposition m_composition;
        MediaClip m_clip;
        MediaOverlay m_overlay;
        MediaOverlayLayer m_overlayLayer;
        VideoEffectDefinition m_vEffectDefinition;
        BasicEffectProperty property;

        public MainPage()
        {
            this.InitializeComponent();
            m_composition = new MediaComposition();
            m_overlayLayer = new MediaOverlayLayer();

            property = new BasicEffectProperty();
            var prop = new PropertySet();
            prop.Add(nameof(BasicEffect.Property), property);
            m_vEffectDefinition = new VideoEffectDefinition(typeof(BasicEffect).FullName, prop);

            m_profile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Vga);
        }

        private void m_seek_Scale_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            property.Scale = 1 + (e.NewValue / m_seek_Scale.Maximum);
        }

        private async void m_btn_openFile_Click(object sender, RoutedEventArgs e)
        {
            var file = await GetFileFromGallery(".mp4");

            if (file != null)
            {
                m_clip = await MediaClip.CreateFromFileAsync(file);

                m_vProperty = m_clip.GetVideoEncodingProperties();
                m_profile.Video.Width = m_vProperty.Width;
                m_profile.Video.Height = m_vProperty.Height;

                m_composition.Clips.Clear();
                m_composition.Clips.Add(m_clip);

                m_clip.VideoEffectDefinitions.Add(m_vEffectDefinition);
                Refresh();
            }
        }
        async Task<StorageFile> GetFileFromGallery(params string[] filters)
        {
            FileOpenPicker _pick = new FileOpenPicker()
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                ViewMode = PickerViewMode.Thumbnail
            };
            foreach (string filter in filters)
                _pick.FileTypeFilter.Add(filter);
            StorageFile file = await _pick.PickSingleFileAsync();
            return file;
        }

        private void Refresh()
        {
            m_mediaElement.SetMediaStreamSource(m_composition.GenerateMediaStreamSource(m_profile));
        }

        private void m_ts_shape_Toggled(object sender, RoutedEventArgs e)
        {
            property.IsShapeActive = m_ts_shape.IsOn;
            Refresh();
        }
    }
}
