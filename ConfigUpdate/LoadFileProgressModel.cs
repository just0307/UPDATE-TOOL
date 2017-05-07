using System.ComponentModel;

namespace InToolSet.ViewModel.NetworkConfiguration
{
    public class LoadFileProgressModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 已完成进程
        /// </summary>
        private double iItemsCompleted = 0.0;

        /// <summary>
        /// 总进程
        /// </summary>
        private double iItemsTotal = 0.0;

        /// <summary>
        /// 进度条式样
        /// </summary>
        private bool m_IsIndeterminate = false;

        /// <summary>
        /// 进度条信息
        /// </summary>
        private string m_strInformation;

        private static LoadFileProgressModel m_instance = new LoadFileProgressModel();
        private static LoadFileProgressModel m_windowInstance = new LoadFileProgressModel();

        public event PropertyChangedEventHandler PropertyChanged;


        public static LoadFileProgressModel Instance
        {
            get
            {
                return m_instance;
            }
        }

        public static LoadFileProgressModel WindowInstance
        {
            get { return m_windowInstance; }
        }

        /// <summary>
        /// 是否正在进程中
        /// </summary>
        public bool Loading
        {
            get
            {
                if (ItemsTotal <= 0)
                {
                    return false;
                }

                return this.iItemsCompleted <= ItemsTotal ? true : false;
            }
        }

        /// <summary>
        /// 已完成进程
        /// </summary>
        public double ItemsCompleted
        {
            get
            {
                if (this.iItemsCompleted > this.ItemsTotal)
                {
                    return this.ItemsTotal;
                }
                return this.iItemsCompleted;
            }
            set
            {
                this.iItemsCompleted = value;
                this.OnPropertyChanged("ItemsCompleted");
                this.OnPropertyChanged("Loading");
            }
        }

        /// <summary>
        /// 总进程
        /// </summary>
        public double ItemsTotal
        {
            get { return this.iItemsTotal; }
            set
            {
                this.iItemsTotal = value;
                this.OnPropertyChanged("ItemsTotal");
                this.OnPropertyChanged("Loading");
            }
        }

        /// <summary>
        /// 属性改变
        /// </summary>
        void OnPropertyChanged(string strProp)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(strProp));
        }

        /// <summary>
        /// 进度条式样
        /// </summary>
        public bool IsIndeterminate
        {
            get { return m_IsIndeterminate; }
            set
            {
                m_IsIndeterminate = value;
                this.OnPropertyChanged("IsIndeterminate");
            }
        }

        /// <summary>
        /// 进度条信息
        /// </summary>
        public string Information
        {
            get { return m_strInformation; }
            set
            {
                m_strInformation = value;
                this.OnPropertyChanged("Information");
            }
        }
    }
}
