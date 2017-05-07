/********************************************************************/
/*  file name    : ExternTextBox.cs                                 */
/*  function     : 扩展文本框                                       */
/*  date/version : 2015/12/11/v1.0                                  */
/*  author       : lvjm                                             */
/********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using InToolSet.Util;
using System.Text.RegularExpressions;

namespace ExternText
{

    /// <summary>
    /// 文本种类
    /// </summary>
    public enum ExternTextFormat
    {
        Text, //文本
        Integer,//整数
        Double,//浮点数
        IPv4,//IPv4地址
        HexInteger,//16进制数
        HexAndInt //16#开头的16进制数
    }

    /// <summary>
    /// 事件处理接口
    /// </summary>
    public interface ITextBoxFormatEvent
    {
        void EventPasting(object sender, DataObjectPastingEventArgs e);
        void EventPreviewKeyDown(object sender, KeyEventArgs e);
        void EventPreviewTextInput(object sender, TextCompositionEventArgs e);
        void EventTextChanged(object sender, TextChangedEventArgs e);
        void EventLoaded(object sender, RoutedEventArgs e);
        void EventGotFocus(object sender, RoutedEventArgs e);
        void EventLostFocus(object sender, RoutedEventArgs e);
        void EventSelectionChanged(object sender, EventArgs e);
    }

    /// <summary>
    /// 扩展文本框

    /// </summary>
    public class ExternTextBox : TextBox
    {
        /// <summary>
        /// 事件对象
        /// </summary>
        private ITextBoxFormatEvent m_eventAdapter = null;

        /// <summary>
        /// 最大值属性

        /// </summary>
        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof(string), typeof(ExternTextBox));

        /// <summary>
        /// 最小值属性

        /// </summary>
        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.RegisterAttached("MinValue", typeof(string), typeof(ExternTextBox),
            new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnMinValueChanged)));

        /// <summary>
        /// 文本类型
        /// </summary>
        public static readonly DependencyProperty FormatProperty =
            DependencyProperty.RegisterAttached("Format", typeof(ExternTextFormat), typeof(ExternTextBox),
            new FrameworkPropertyMetadata(ExternTextFormat.Text, new PropertyChangedCallback(OnFormatChanged)));

        /// <summary>
        /// 最小值属性

        /// </summary>
        public static readonly DependencyProperty MinLengthProperty =
            DependencyProperty.Register("MinLength", typeof(int), typeof(ExternTextBox));

        /// <summary>
        /// 是否显示tooltip
        /// </summary>
        public static readonly DependencyProperty ShowToolTipProperty =
            DependencyProperty.Register("ShowToolTip", typeof(bool), typeof(ExternTextBox), new PropertyMetadata(true));

        /// <summary>
        /// 是否允许输入空字符串
        /// </summary>
        public static readonly DependencyProperty CanEmptyProperty =
            DependencyProperty.Register("CanEmpty", typeof(bool), typeof(ExternTextBox), new PropertyMetadata(false));

        public ExternTextBox()
        {
            DataObject.AddPastingHandler(this, ExternTextBox_Pasting);
            this.PreviewKeyDown += ExternTextBox_PreviewKeyDown;
            this.PreviewTextInput += ExternTextBox_PreviewTextInput;
            this.TextChanged += ExternTextBox_TextChanged;
            this.Loaded += ExternTextBox_Loaded;
            this.GotFocus += ExternTextBox_GotFocus;
            this.LostFocus += ExternTextBox_LostFocus;
            this.SelectionChanged += ExternTextBox_SelectionChanged;
        }

        private void ExternTextBox_SelectionChanged(object sender, EventArgs e)
        {
            EventAdapter.EventSelectionChanged(sender, e);
        }

        /// <summary>
        /// Text控件内粘贴事件

        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExternTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            EventAdapter.EventPasting(sender, e);
        }

        /// <summary>
        /// Text控件内键盘按下事件

        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExternTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            EventAdapter.EventPreviewKeyDown(sender, e);
        }

        /// <summary>
        /// Text控件内输入事件

        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExternTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            EventAdapter.EventPreviewTextInput(sender, e);
        }

        public void ExternTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render,
                               new Action(() =>
                               {
                                   EventAdapter.EventTextChanged(sender, e);
                               }));
        }

        public void ExternTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            EventAdapter.EventLoaded(sender, e);
            ExternTextBox_TextChanged(this, null);
        }

        public void ExternTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            EventAdapter.EventGotFocus(sender, e);
        }
        public void ExternTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            EventAdapter.EventLostFocus(sender, e);
        }

        /// <summary>
        /// 最大值

        /// </summary>
        public string MaxValue
        {
            get { return (string)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        /// <summary>
        /// 最小值

        /// </summary>
        public string MinValue
        {
            get { return (string)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }

        /// <summary>
        /// 字符串最小长度

        /// </summary>
        public int MinLength
        {
            get { return (int)GetValue(MinLengthProperty); }
            set { SetValue(MinLengthProperty, value); }
        }

        /// <summary>
        /// 文本类型
        /// </summary>
        public ExternTextFormat Format
        {
            get { return (ExternTextFormat)GetValue(FormatProperty); }
            set
            {
                EventAdapter = null;
                SetValue(FormatProperty, value);
            }
        }

        /// <summary>
        /// 是否允许输入空字符串
        /// </summary>
        public bool CanEmpty
        {
            get { return (bool)GetValue(CanEmptyProperty); }
            set
            {
                SetValue(CanEmptyProperty, value);
            }
        }

        /// <summary>
        /// 是否显示tooltip
        /// </summary>
        public bool ShowToolTip
        {
            get { return (bool)GetValue(ShowToolTipProperty); }
            set
            {
                SetValue(ShowToolTipProperty, value);
            }
        }

        private static void OnFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ExternTextBox textBox = d as ExternTextBox;
            if (textBox != null)
            {
                textBox.EventAdapter = null;
            }
        }

        private static void OnMinValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ExternTextBox textBox = d as ExternTextBox;
            if (textBox != null)
            {
                textBox.ToolTip = string.Format("{0} ~ {1}", textBox.MinValue, textBox.MaxValue);
            }
        }

        /// <summary>
        /// 获得事件处理对象
        /// </summary>
        public ITextBoxFormatEvent EventAdapter
        {
            get
            {
                if (null == m_eventAdapter)
                {
                    switch (Format)
                    {
                        case ExternTextFormat.Integer:
                            m_eventAdapter = new TextBoxFormatInteger(this);
                            break;
                        case ExternTextFormat.Double:
                            m_eventAdapter = new TextBoxFormatDouble(this);
                            break;
                        case ExternTextFormat.IPv4:
                            m_eventAdapter = new TextBoxFormatIPv4(this);
                            break;
                        case ExternTextFormat.HexInteger:
                            m_eventAdapter = new TextBoxFormatHexInt(this);
                            break;
                        case ExternTextFormat.HexAndInt:
                            m_eventAdapter = new TextBoxFormatHexAndInt(this);
                            break;
                        default:
                            m_eventAdapter = new TextBoxFormatText(this);
                            break;
                    }
                }

                return m_eventAdapter;
            }
            set
            {
                m_eventAdapter = value;
            }
        }
    }

    /// <summary>
    /// 文本处理类

    /// </summary>
    public class TextBoxFormatText : ITextBoxFormatEvent
    {
        protected ExternTextBox m_textBox;
        protected string m_strOldText = string.Empty;

        public TextBoxFormatText(ExternTextBox textBox)
        {
            m_textBox = textBox;
        }

        public virtual void EventLoaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(m_textBox.MinValue) ||
                !string.IsNullOrEmpty(m_textBox.MaxValue))
            {
                m_textBox.ToolTip = string.Format("{0} ~ {1}", m_textBox.MinValue, m_textBox.MaxValue);
            }

            if (!m_textBox.ShowToolTip)
            {
                m_textBox.ToolTip = null;
            }

            m_strOldText = m_textBox.MinValue;
        }

        /// <summary>
        /// Text控件内粘贴事件

        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void EventPasting(object sender, DataObjectPastingEventArgs e)
        {
        }

        /// <summary>
        /// Text控件内键盘按下事件

        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void EventPreviewKeyDown(object sender, KeyEventArgs e)
        {
            m_strOldText = m_textBox.Text;
        }

        /// <summary>
        /// Text控件内输入事件

        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void EventPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
        }

        /// <summary>
        /// 文本内容发生变化处理事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void EventTextChanged(object sender, TextChangedEventArgs e)
        {
        }

        public virtual void EventGotFocus(object sender, RoutedEventArgs e)
        {

        }

        public virtual void EventSelectionChanged(object sender, EventArgs e)
        {

        }

        public virtual void EventLostFocus(object sender, RoutedEventArgs e)
        {

        }
    }

    /// <summary>
    /// 浮点型处理类
    /// </summary>
    public class TextBoxFormatDouble : TextBoxFormatText
    {
        public TextBoxFormatDouble(ExternTextBox textBox)
            : base(textBox)
        {
            InputMethod.SetIsInputMethodEnabled(textBox, false);
            if (string.IsNullOrEmpty(textBox.MaxValue))
            {
                textBox.MaxValue = Int64.MaxValue.ToString();
            }
            if (string.IsNullOrEmpty(textBox.MinValue))
            {
                textBox.MinValue = Int64.MinValue.ToString();
            }

            textBox.MinLength = textBox.MinValue.Length;
            //textBox.MaxLength = textBox.MaxValue.Length + 1;
            textBox.ToolTip = string.Format("{0} ~ {1}", textBox.MinValue, textBox.MaxValue);
            m_strOldText = textBox.MinValue;
        }

        /// <summary>
        /// Text控件内粘贴事件

        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void EventPasting(object sender, DataObjectPastingEventArgs e)
        {
            // 整数以外禁止粘贴
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!GlobalUtil.IsInteger(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        /// <summary>
        /// Text控件内键盘按下事件

        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void EventPreviewKeyDown(object sender, KeyEventArgs e)
        {
            m_strOldText = m_textBox.Text;
            if (string.IsNullOrEmpty(m_strOldText))
            {
                m_strOldText = m_textBox.MinValue;
            }

            // 禁止空格键输入

            if (e.Key == Key.Space)
            {
                e.Handled = true;
                return;
            }

            if (((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Shift) == (System.Windows.Forms.Keys.Shift)) ||
                (((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Control) == (System.Windows.Forms.Keys.Control))) ||
                (((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Alt) == (System.Windows.Forms.Keys.Alt))))
            {
                //控制键按下时，不特殊处理
                return;
            }

            if (e.Key == Key.Left || e.Key == Key.Right)
            {
                TextBox textBox = sender as TextBox;
                double dMaxValue = Int64.MaxValue;
                double dMinValue = Int64.MinValue + 1;
                double.TryParse((m_textBox.MinValue), out dMinValue);
                double.TryParse((m_textBox.MaxValue), out dMaxValue);
                double dValue = dMinValue - 1;
                if (!string.IsNullOrEmpty(textBox.Text))
                {
                    //值非空时，转化整型值

                    double.TryParse((textBox.Text), out dValue);
                    if (dMaxValue / dValue < 10)
                    {
                        int iSelIdx = textBox.SelectionStart;
                        if (e.Key == Key.Left)
                        {
                            if (iSelIdx - 1 >= 0)
                            {
                                textBox.SelectionStart = iSelIdx - 1;
                            }
                        }
                        else
                        {
                            if (iSelIdx + 1 <= textBox.Text.Length)
                            {
                                textBox.SelectionStart = iSelIdx + 1;
                            }
                        }
                        textBox.SelectionLength = 1;
                        e.Handled = true;
                    }
                }
            }
        }

        /// <summary>
        /// Text控件内输入事件

        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void EventPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 禁止整数以外输入
            if (!GlobalUtil.IsInteger(e.Text) && e.Text != "-" && e.Text != ".")
            {
                e.Handled = true;
            }
            else
            {
                TextBox textBox = sender as TextBox;
                if (e.Text == "." && textBox.Text.IndexOf(".") >= 0)
                {
                    //[.]已经存在时，忽略
                    e.Handled = true;
                    return;
                }
                double dMinValue;
                double.TryParse((m_textBox.MinValue), out dMinValue);
                if ((e.Text == "-") && ((dMinValue >= 0) || (textBox.SelectionStart > 0)))
                {
                    //[-]不是在最前面时，忽略
                    e.Handled = true;
                    return;
                }
                e.Handled = false;
            }
        }

        /// <summary>
        /// 文本内容发生变化处理事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void EventTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (m_textBox.CanEmpty && string.IsNullOrEmpty(textBox.Text))
            {
                if (e != null)
                {
                    e.Handled = true;
                }
                return;
            }

            double dMaxValue = Int64.MaxValue;
            double dMinValue = Int64.MinValue + 1;
            double.TryParse((m_textBox.MinValue), out dMinValue);
            double.TryParse((m_textBox.MaxValue), out dMaxValue);
            double dValue = dMinValue - 1;
            int iSelIdx;
            string strDecimalPart = string.Empty;
            int iDecIdx = -1;
            if (!string.IsNullOrEmpty(textBox.Text))
            {
                //值非空时，转化整型值

                double.TryParse((textBox.Text), out dValue);
                iDecIdx = textBox.Text.IndexOf(".");
                if (iDecIdx >= 0)
                {
                    strDecimalPart = textBox.Text.Substring(iDecIdx + 1);
                }
            }
            if ( string.IsNullOrEmpty(textBox.Text) || /*dValue < dMinValue || dValue > dMaxValue ||*/ strDecimalPart.Length > 15)
            {
                //值是空或者范围外,或者小数部分的值大于九位时，设定回修改前的值
                if (e != null)
                {
                    e.Handled = true;
                }

                if(string.IsNullOrEmpty(textBox.Text))
                {
                    return;
                }

                iSelIdx = textBox.SelectionStart;
                textBox.Text = m_strOldText;
                if (iSelIdx >= 0 && m_strOldText.Length > iSelIdx)
                {
                    textBox.SelectionStart = iSelIdx;
                }
                else
                {
                    if (m_strOldText.Length >= m_textBox.MaxValue.Split('.')[0].Length)
                    {
                        textBox.SelectionStart = m_strOldText.Length - 1;
                    }
                    else
                    {
                        textBox.SelectionStart = m_strOldText.Length;
                    }
                }
                textBox.SelectionLength = 1;
                if (!textBox.IsFocused)
                {
                    BindingExpression bindingExpresion = textBox.GetBindingExpression(TextBox.TextProperty);
                    if (bindingExpresion != null)
                    {
                        bindingExpresion.UpdateSource();
                    }
                }
                return;
            }
            else
            {
                // 显示值不是整型值时，设定成整型值
                string strFormatValue = ((Int64)dValue).ToString();
                bool bAdded = false;
                if (m_textBox.Format != ExternTextFormat.Integer && iDecIdx >= 0)
                {
                    strFormatValue += '.';
                    if (string.IsNullOrEmpty(strDecimalPart))
                    {
                        strFormatValue += "0";
                        bAdded = true;
                    }
                    else
                    {
                        strFormatValue += strDecimalPart;
                    }
                }

                if (!string.IsNullOrEmpty(strFormatValue) && textBox.Text != strFormatValue)
                {
                    textBox.Text = strFormatValue;
                    if (bAdded)
                    {
                        textBox.SelectionStart = strFormatValue.Length - 1;
                        textBox.SelectionLength = 1;
                    }
                    else
                    {
                        textBox.SelectionStart = strFormatValue.Length;
                    }
                }

                iSelIdx = textBox.SelectionStart;
                if (dMaxValue / dValue < 10)
                {
                    if (iDecIdx > 0 && iSelIdx > iDecIdx)
                    {
                        return;
                    }

                    if (iSelIdx >= textBox.Text.Length)
                    {
                        textBox.SelectionStart = textBox.Text.Length - 1;
                    }
                    textBox.SelectionLength = 1;
                }
            }
        }

        public override void EventLostFocus(object sender, RoutedEventArgs e)
        {
            double dMaxValue = Int64.MaxValue;
            double dMinValue = Int64.MinValue + 1;
            double.TryParse((m_textBox.MinValue), out dMinValue);
            double.TryParse((m_textBox.MaxValue), out dMaxValue);
            double dValue = dMinValue - 1;
            string strText = m_textBox.Text;
            bool bOutOfRange = false;
            if (!string.IsNullOrEmpty(strText))
            {
                double.TryParse(strText, out dValue);
                if(dMinValue > dValue)
                {
                    bOutOfRange = true;
                    strText = m_textBox.MinValue;
                }
                if(dValue > dMaxValue)
                {
                    bOutOfRange = true;
                    strText = m_textBox.MaxValue;
                }
            }
            else
            {
                bOutOfRange = !m_textBox.CanEmpty;
                strText = m_textBox.MinValue;
            }

            if (bOutOfRange)
            {
                MessageBox.Show("超出范围");
                m_textBox.Text = strText;
            }
        }
    }

    /// <summary>
    /// 整型处理类

    /// </summary>
    public class TextBoxFormatInteger : TextBoxFormatDouble
    {
        /// <summary>
        /// 构造函数

        /// </summary>
        /// <param name="textBox"></param>
        public TextBoxFormatInteger(ExternTextBox textBox)
            : base(textBox)
        {
        }

        /// <summary>
        /// 文本内容发生变化处理事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void EventTextChanged(object sender, TextChangedEventArgs e)
        {
            base.EventTextChanged(sender, e);
            TextBox textBox = sender as TextBox;
            if ((e != null && e.Handled) || (m_textBox.CanEmpty && string.IsNullOrEmpty(textBox.Text)))
            {
                return;
            }

            double dValue;
            if (double.TryParse((textBox.Text), out dValue))
            {
                // 显示值不是整型值时，设定成整型值

                string strFormatValue = Convert.ToInt64(dValue).ToString();
                if (textBox.Text != strFormatValue)
                {
                    textBox.Text = strFormatValue;
                    textBox.SelectionStart = strFormatValue.Length;
                }
            }
        }
    }

    /// <summary>
    /// 整型处理类

    /// </summary>
    public class TextBoxFormatIPv4 : TextBoxFormatText
    {
        /// <summary>
        /// 按下的键
        /// </summary>
        Key m_processedKey = Key.None;
        bool m_bSelChanging = false;
        bool m_bSelAllSubText = false;
        bool m_bAfterGotFoucs = false;
        int m_iSubSectiongLength = 5;

        /// <summary>
        /// 构造函数

        /// </summary>
        /// <param name="textBox"></param>
        public TextBoxFormatIPv4(ExternTextBox textBox)
            : base(textBox)
        {
            InputMethod.SetIsInputMethodEnabled(textBox, false);
            if (string.IsNullOrEmpty(m_textBox.MinValue))
            {
                m_textBox.MinValue = "0.0.0.0";
            }

            if (string.IsNullOrEmpty(m_textBox.MaxValue))
            {
                m_textBox.MaxValue = "255.255.255.255";
            }
        }

        /// <summary>
        /// Text控件内粘贴事件

        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void EventPasting(object sender, DataObjectPastingEventArgs e)
        {

        }

        /// <summary>
        /// Text控件内键盘按下事件

        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void EventPreviewKeyDown(object sender, KeyEventArgs e)
        {
            m_processedKey = e.Key;
            base.EventPreviewKeyDown(sender, e);
            if (e.Handled)
            {
                return;
            }

            if (((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Shift) == (System.Windows.Forms.Keys.Shift)) ||
                (((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Control) == (System.Windows.Forms.Keys.Control))) ||
                (((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Alt) == (System.Windows.Forms.Keys.Alt))))
            {
                //控制键按下时，不特殊处理
                return;
            }

            TextBox textBox = sender as TextBox;
            if (null == textBox)
            {
                return;
            }

            string strText = textBox.Text;
            if (!string.IsNullOrEmpty(strText))
            {
                if (e.Key == Key.Left || e.Key == Key.Right)
                {
                    int iSelIdx = textBox.SelectionStart;
                    int iOldSelIdx = iSelIdx;
                    if (e.Key == Key.Left)
                    {
                        int iSkipDotCnt = 0;
                        while (iSelIdx - 1>=0 && (strText[iSelIdx - 1] == '.'||strText[iSelIdx - 1] == ' '))
                        {
                            if(strText[iSelIdx - 1] == '.')
                            {
                                iSkipDotCnt++;
                            }
                            if (iSkipDotCnt > 1)
                            {
                                break;
                            }
                            iSelIdx -= 1;
                        }
                    }
                    else
                    {
                        int iSkipDotCnt = 0;
                        while ((iSelIdx < strText.Length) && (strText[iSelIdx] == '.' || strText[iSelIdx] == ' '))
                        {
                            if (strText[iSelIdx] == '.')
                            {
                                iSkipDotCnt++;
                            }
                            if (iSkipDotCnt > 1)
                            {
                                break;
                            }
                            iSelIdx += 1;
                        }
                    }
                    if (iSelIdx != iOldSelIdx)
                    {
                        textBox.SelectionStart = iSelIdx;
                        textBox.SelectionLength = 0;
                        e.Handled = true;
                    }
                    return;
                }

                if (e.Key == Key.Back)
                {
                    int iSelIdx = textBox.SelectionStart;
                    //if (iSelIdx == 0)
                    //{
                    //    //光标在最前面，点击back时，选中第一个字符
                    //    textBox.SelectionLength = 1;
                    //    e.Handled = true;
                    //    return;
                    //}
                    int iOldSelIdx = iSelIdx;
                    while (iSelIdx - 1 >= 0 && (strText[iSelIdx - 1] == '.' || strText[iSelIdx - 1] == ' '))
                    {
                        iSelIdx -= 1;
                    }
                    if (iSelIdx != iOldSelIdx)
                    {
                        textBox.SelectionStart = iSelIdx;
                        textBox.SelectionLength = 0;
                        e.Handled = true;
                    }
                    //if (iSelIdx < strText.Length && iSelIdx - 1 >= 0 && textBox.Text[iSelIdx - 1] == '.')
                    //{
                    //    //在[.]的位置按下back键时，光标向前移一位
                    //    textBox.SelectionStart = iSelIdx - 1;
                    //    e.Handled = true;
                    //    return;
                    //}
                }
            }
        }

        /// <summary>
        /// Text控件内输入事件

        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void EventPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            base.EventPreviewTextInput(sender, e);
            if (e.Handled)
            {
                return;
            }

            // 禁止数字、[.]以外输入
            if (!GlobalUtil.IsInteger(e.Text) && e.Text != ".")
            {
                e.Handled = true;
            }
            else
            {
                TextBox textBox = (sender as TextBox);
                if (textBox == null)
                {
                    return;
                }
                int iSelIdx = textBox.SelectionStart;
                if (e.Text != ".")
                {
                    //数字
                    if (iSelIdx == m_iSubSectiongLength*4 - 1)
                    {
                        //输入数字，光标位于最后
                        if (textBox.Text[iSelIdx - 1] != ' ')
                        {
                            //如果最后一个字符不是空白，忽略
                            e.Handled = true;
                            return;
                        }
                    }
                    e.Handled = false;
                    return;
                }

                //输入[.]时，移动光标位置
                if (textBox.SelectionLength > 0)
                {
                    //do nothing
                }
                else
                {
                    int iDivCnt = textBox.Text.Substring(0, iSelIdx).Split('.').Count();
                    if (iDivCnt <= 3)
                    {
                        textBox.Text = IPv4TextFormat(textBox.Text);
                        textBox.SelectionStart = (iDivCnt) * m_iSubSectiongLength;
                        textBox.SelectionLength = 3;
                    }
                    else
                    {
                        textBox.SelectionStart = textBox.Text.Length;
                        textBox.SelectionLength = 0;
                    }
                }

                e.Handled = true;
            }
        }

        /// <summary>
        /// 文本内容发生变化处理事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void EventTextChanged(object sender, TextChangedEventArgs e)
        {
            base.EventTextChanged(sender, e);
            if (e != null && e.Handled)
            {
                return;
            }

            TextBox textBox = sender as TextBox;
            int iSelIdx = textBox.SelectionStart;
            List<string> origSubList = textBox.Text.Split('.').ToList();
            string strFormat = IPv4TextFormat(textBox.Text);
            m_bSelAllSubText = false;
            //if (GlobalUtil.CompareIPv4(strFormat, m_textBox.MinValue) < 0 ||
            //    GlobalUtil.CompareIPv4(strFormat, m_textBox.MaxValue) > 0)
            //{
            //    m_textBox.Text = IPv4TextFormat(m_strOldText);
            //    if (e != null)
            //    {
            //        e.Handled = true;
            //    }
            //    return;
            //}

            if (strFormat != textBox.Text)
            {
                textBox.Text = strFormat;
                if (iSelIdx < strFormat.Length)
                {
                    List<string> subList = strFormat.Split('.').ToList();

                    int iSubSectionIdx = iSelIdx / m_iSubSectiongLength;
                    bool bSelNextSec = false;
                    if (subList[iSubSectionIdx].Trim().Length >= 3 || !m_bAfterGotFoucs)
                    {
                        //输入第三位数字时，移动光标位置到下一个字段
                        if (iSubSectionIdx < 3)
                        {
                            bSelNextSec = true;
                        }
                    }

                    if (bSelNextSec)
                    {
                        m_bSelAllSubText = true;
                        if (m_bAfterGotFoucs)
                        {
                            textBox.SelectionStart = (iSubSectionIdx + 1) * m_iSubSectiongLength;
                        }
                        else
                        {
                            textBox.SelectionStart = (iSubSectionIdx) * m_iSubSectiongLength;
                        }
                    }
                    else
                    {
                        int iSelAdjust = 0;
                        if (origSubList[iSubSectionIdx].Length > 0 && origSubList[iSubSectionIdx][0] == ' ' && subList[iSubSectionIdx][0] != ' ')
                        {
                            iSelAdjust = -1;
                        }
                        if (origSubList[iSubSectionIdx].Length > 0 && origSubList[iSubSectionIdx][0] != ' ' && subList[iSubSectionIdx][0] == ' ')
                        {
                            iSelAdjust = 1;
                        }
                        textBox.SelectionStart = iSelIdx + iSelAdjust;
                    }

                    //while ((iSelIdx - 1 >= 0) && (strFormat[iSelIdx - 1] == ' '))
                    //{
                    //    iSelIdx--;
                    //}

                    //if ((iSelIdx + 1) < strFormat.Length && strFormat[iSelIdx] == '.')
                    //{
                    //    if (m_processedKey != Key.Back)
                    //    {
                    //        //输入第三位数字时，移动光标位置
                    //        iSelIdx += 1;
                    //    }
                    //    else
                    //    {
                    //        if (iSelIdx - 1 > 0)
                    //        {
                    //            //按下back键时，向前移动
                    //            iSelIdx -= 1;
                    //        }
                    //    }
                    //}
                    //textBox.SelectionStart = iSelIdx;
                }
                else
                {
                    textBox.SelectionStart = strFormat.Length;
                }
            }

            //选中[.]之后的0
            if ((iSelIdx < strFormat.Length && strFormat[iSelIdx] == '0' &&
                iSelIdx - 1 >= 0 && strFormat[iSelIdx - 1] == '.' && m_processedKey != Key.Back))
            {
                textBox.SelectionLength = 1;
            }
        }

        /// <summary>
        /// 格式化文本

        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        public string IPv4TextFormat(string strText, string strDefaultValue = "")
        {
            List<string> list;
            if (!string.IsNullOrEmpty(strText))
            {
                list = strText.Replace(" ", "").Split('.').ToList();
            }
            else
            {
                list = new List<string>();
            }

            while (list.Count < 4)
            {
                list.Add("    ");
            }

            string strRet = string.Empty;
            for (int iIdx = 0; iIdx < 4; iIdx++)
            {
                string strTmp = list[iIdx].Trim();
                int iSubValue = 0;
                if (int.TryParse(strTmp, out iSubValue))
                {
                    while (iSubValue >= 1000)
                    {
                        iSubValue /= 10;
                    }

                    if (iSubValue > 255)
                    {
                        iSubValue = 255;
                    }
                    strTmp = iSubValue.ToString();
                }
                else
                {
                    strTmp = strDefaultValue;
                }

                while(strTmp.Length <= 1)
                {
                    strTmp = " " + strTmp;
                }

                while (strTmp.Length < 4)
                {
                    strTmp += " ";
                }

                if (string.IsNullOrEmpty(strRet))
                {
                    strRet += strTmp;
                }
                else
                {
                    strRet += string.Format(".{0}", strTmp);
                }
            }
            return strRet;
        }

        /// <summary>
        /// 去掉格式化字符
        /// </summary>
        /// <param name="strIPv4">IPv4地址</param>
        /// <returns>格式化之后的字符串</returns>
        public static string GetRealIPv4String(string strIPv4)
        {
            if (string.IsNullOrEmpty(strIPv4))
            {
                return string.Empty;
            }
            return strIPv4.Replace(" ", "");
        }

        public override void EventGotFocus(object sender, RoutedEventArgs e)
        {
            m_bSelAllSubText = true;
            m_bAfterGotFoucs = true;
        }

        public override void EventSelectionChanged(object sender, EventArgs e)
        {
            UpdateSelPos(sender as TextBox);
        }
        protected void UpdateSelPos(TextBox textBox)
        {
            if (null == textBox || m_bSelChanging)
            {
                return;
            }

            m_bSelChanging = true;
            string strText = textBox.Text;
            List<string> list = strText.Replace(" ", "").Split('.').ToList();
            int iSelStart = textBox.SelectionStart;
            int iSelSection = iSelStart / 5;
            if(iSelSection < 0 || iSelSection > 3)
            {
                iSelSection = 0;
            }
            if(iSelSection > 3)
            {
                iSelSection = 3;
            }

            if (m_bSelAllSubText || string.IsNullOrEmpty(list[iSelSection]))
            {
                int iSubStart = 0;
                string strSelSectionString = list[iSelSection];
                if (string.IsNullOrEmpty(strSelSectionString))
                {
                    //光标放在中间
                    iSubStart = 2;
                }
                else if (strSelSectionString.Length == 1)
                {
                    //选中所有字符
                    iSubStart = 1;
                }
                else
                {
                    // do nothing
                }
                textBox.SelectionStart = iSelSection * 5 + iSubStart;
                textBox.SelectionLength = list[iSelSection].Length;
                m_bSelAllSubText = false;
            }
            else
            {
                int iOldSetStart = iSelStart;
                if(iSelStart > 0&&strText[iSelStart-1] == ' ')
                {
                    if (iSelStart == strText.Length || strText[iSelStart] == ' ')
                    {
                        while (iSelStart > 0&&strText[iSelStart - 1] == ' ')
                        {
                            iSelStart--;
                        }
                    }
                }
                if (strText[iSelStart] == ' ' || strText[iSelStart] == '.')
                {
                    if (iSelStart == 0 || iSelStart > 0 && strText[iSelStart-1] == '.' || strText[iSelStart] == '.')
                    {
                        while ((iSelStart < strText.Length && (strText[iSelStart] == ' ' || strText[iSelStart] == '.')))
                        {
                            iSelStart++;
                        }
                    }
                }
                if(iOldSetStart != iSelStart)
                {
                    textBox.SelectionStart = iSelStart;
                }
            }
            iSelStart = textBox.SelectionStart;
            int iSelLength = textBox.SelectionLength;
            if (iSelLength > 0)
            {
                while ((iSelStart + iSelLength - 1>=0)&&(strText[iSelStart + iSelLength - 1] == ' ' || strText[iSelStart + iSelLength - 1] == '.'))
                {
                    iSelLength--;
                }

                if(iSelLength < 0)
                {
                    iSelLength = 0;
                }

                if(textBox.SelectionLength != iSelLength)
                {
                    textBox.SelectionLength = iSelLength;
                }
            }
            m_bSelChanging = false;
        }

        public override void EventLostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if(textBox == null)
            {
                return;
            }

           // if (m_bSelAllSubText)
            {
                m_bAfterGotFoucs = false;
                m_bSelAllSubText = false;
            }
            string strText = IPv4TextFormat(textBox.Text, "0");
            bool bOutOfRange = false;
            if (GlobalUtil.CompareIPv4(strText, m_textBox.MinValue) < 0)
            {
                strText = m_textBox.MinValue;
                bOutOfRange = true;
            }
            else if(GlobalUtil.CompareIPv4(strText, m_textBox.MaxValue) > 0)
            {
                strText = m_textBox.MaxValue;
                bOutOfRange = true;
            }
            else
            {
                //do nothing
            }

            if (bOutOfRange)
            {
                MessageBox.Show("超出范围");
            }

            if (strText != IPv4TextFormat(textBox.Text))
            {
                textBox.Text = strText;
            }
        }
    }
    
    /// <summary>
    /// 文本处理类
    /// </summary>
    public class TextBoxFormatHexInt : TextBoxFormatText
    {
        public TextBoxFormatHexInt(ExternTextBox textBox)
            : base(textBox)
        {
        }

        /// <summary>
        /// Text控件内粘贴事件

        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void EventPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!IsHexChar(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
        
        /// <summary>
        /// Text控件内输入事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void EventPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!IsHexChar(e.Text))
            {
                e.Handled = true;
            }
        }
        
        public bool IsHexChar(string strText)
        {
            return Regex.IsMatch(strText, "^[0-9a-fA-F]*$");
        }
    }

    /// <summary>
    /// 文本处理类
    /// </summary>
    public class TextBoxFormatHexAndInt : TextBoxFormatText
    {
        public TextBoxFormatHexAndInt(ExternTextBox textBox)
            : base(textBox)
        {
        }

        /// <summary>
        /// Text控件内粘贴事件

        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void EventPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!IsValidChar(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        /// <summary>
        /// Text控件内输入事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void EventPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!IsValidChar(e.Text))
            {
                e.Handled = true;
            }
        }

        public bool IsValidChar(string strText)
        {
            return Regex.IsMatch(strText, "^16#[0-9a-fA-F]*$|[0-9]*$");
        }

        public override void EventLostFocus(object sender, RoutedEventArgs e)
        {
            double dMaxValue = Int64.MaxValue;
            double dMinValue = Int64.MinValue + 1;
            double.TryParse((m_textBox.MinValue), out dMinValue);
            double.TryParse((m_textBox.MaxValue), out dMaxValue);
            double dValue = dMinValue - 1;
            string strText = m_textBox.Text;
            bool bOutOfRange = false;
            if (!string.IsNullOrEmpty(strText))
            {
                if (strText.StartsWith("16#"))
                {
                    strText = strText.Substring(3);
                    if (string.IsNullOrEmpty(strText))
                    {
                        bOutOfRange = !m_textBox.CanEmpty;
                        strText = m_textBox.MinValue;
                    }
                    else
                    {
                        dValue = Convert.ToInt64(strText, 16);
                        if (dMinValue > dValue)
                        {
                            bOutOfRange = true;
                            strText = m_textBox.MinValue;
                        }
                        if (dValue > dMaxValue)
                        {
                            bOutOfRange = true;
                            strText = m_textBox.MaxValue;
                        }
                    }
                }
                else
                {
                    double.TryParse(strText, out dValue);
                    if (dMinValue > dValue)
                    {
                        bOutOfRange = true;
                        strText = m_textBox.MinValue;
                    }
                    if (dValue > dMaxValue)
                    {
                        bOutOfRange = true;
                        strText = m_textBox.MaxValue;
                    }
                }
            }
            else
            {
                bOutOfRange = !m_textBox.CanEmpty;
                strText = m_textBox.MinValue;
            }

            if (bOutOfRange)
            {
                //UserMessage.PromptError(string.Format(new Loc("Message:Common_OutofRange").Text, m_textBox.MinValue, m_textBox.MaxValue));
                m_textBox.Text = strText;
            }
        }
    }
}
