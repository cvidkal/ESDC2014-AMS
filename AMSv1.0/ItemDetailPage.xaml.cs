using AMSv1._0.Common;
using AMSv1._0.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.Networking;
using Windows.UI.Xaml.Media.Imaging;


// “项详细信息页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234232 上提供

namespace AMSv1._0
{
    /// <summary>
    /// 显示组内某一项的详细信息的页面。
    /// </summary>
    public sealed partial class ItemDetailPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        public StreamSocket socket;
        public DataWriter writer;
        public DataReader reader;
        private const int size=147510;

        byte[] read = new byte[size];
        /// <summary>
        /// NavigationHelper 在每页上用于协助导航和
        /// 进程生命期管理
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// 可将其更改为强类型视图模型。
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        public ItemDetailPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;

        }

        private async void connect_camera(byte cameranum)
        {
            if (cameranum == 0)
                pageTitle.Text = "海面景色";
            else if (cameranum == 1)
                pageTitle.Text = "舱内视角";
            else if (cameranum == 2)
                pageTitle.Text = "甲板视角";
            else if (cameranum == 3)
                pageTitle.Text = "侧舷视角";
            try
            {
                socket = new StreamSocket();
                await socket.ConnectAsync(new HostName("127.0.0.1"), "9900");
                LogInfo.Text = "连接成功";
                writer = new DataWriter(socket.OutputStream);
                writer.WriteByte(cameranum);
                await writer.StoreAsync();
                LogInfo.Text = "正在初始化，请稍等";
                reader = new DataReader(socket.InputStream);
                reader.InputStreamOptions = InputStreamOptions.Partial;
                await reader.LoadAsync(size);
                LogInfo.Text = "初始化完成，工作正常";
                while (true)
                {
                    while (reader.UnconsumedBufferLength < size)
                    {
                        await reader.LoadAsync(size - reader.UnconsumedBufferLength);
                    }

                    reader.ReadBytes(read);
                    MemoryStream stream = new MemoryStream(read);
                    BitmapImage bitmapImage = new BitmapImage();
                    await bitmapImage.SetSourceAsync(stream.AsRandomAccessStream());
                    SeaImage.Source = bitmapImage;
                }
            }
            catch (Exception ex)
            {
                LogInfo.Text = "连接服务器错误";
            }
        }
        

        /// <summary>
        /// 使用在导航过程中传递的内容填充页。  在从以前的会话
        /// 重新创建页时，也会提供任何已保存状态。
        /// </summary>
        /// <param name="sender">
        /// 事件的来源; 通常为 <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">事件数据，其中既提供在最初请求此页时传递给
        /// <see cref="Frame.Navigate(Type, Object)"/> 的导航参数，又提供
        /// 此页在以前会话期间保留的状态的
        /// 字典。 首次访问页面时，该状态将为 null。</param>
        private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // TODO:  创建适用于问题域的合适数据模型以替换示例数据
            var item = await SampleDataSource.GetItemAsync((String)e.NavigationParameter);
            this.DefaultViewModel["Item"] = item;
        }

        #region NavigationHelper 注册

        /// 此部分中提供的方法只是用于使
        /// NavigationHelper 可响应页面的导航方法。
        /// 
        /// 应将页面特有的逻辑放入用于
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// 和 <see cref="GridCS.Common.NavigationHelper.SaveState"/> 的事件处理程序中。
        /// 除了在会话期间保留的页面状态之外
        /// LoadState 方法中还提供导航参数。


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
            string uniqueId = e.Parameter as String;
            if (uniqueId == "Group-1-Item-1")
               connect_camera((byte)0);
            else if (uniqueId == "Group-1-Item-2")
                connect_camera((byte)1);
            else if (uniqueId == "Group-1-Item-3")
                connect_camera((byte)2);
            else if (uniqueId == "Group-1-Item-4")
                connect_camera((byte)3);
                
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);

        }

        #endregion

        private void SeaBack_Click(object sender, RoutedEventArgs e)
        {
            if(Frame.CanGoBack)
            {
                Frame.GoBack();
                socket.Dispose();
            }
        }

    }
}