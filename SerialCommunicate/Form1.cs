using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO.Ports;
namespace SerialCommunicate
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.Close();
                }
                catch { }
                ovalShape1.FillColor = Color.Gray;
                button1.Text = "打开端口";
            }
            else
            {
                try
                {
                    serialPort1.PortName = comboBox1.Text;
                    serialPort1.BaudRate = Convert.ToInt32(comboBox2.Text, 10);//十进制数据转换
                    serialPort1.Open();
                    ovalShape1.FillColor = Color.Green;
                    button1.Text = "关闭端口";
                }
                catch
                {
                    MessageBox.Show("端口错误,请检查串口", "错误");
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ovalShape1.FillColor = Color.Gray;
            SearchAndAddSerialToComboBox(serialPort1, comboBox1);//扫描可用端口
            comboBox2.Text = "4800";//波特率默认值
            
            /*****************非常重要************************/
            
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);//必须手动添加事件处理程序

        }
        private void SearchAndAddSerialToComboBox(SerialPort MyPort, ComboBox MyBox)//扫描可用端口
        {                                                               //将可用端口号添加到ComboBox
            string Buffer;                                              //缓存
            MyBox.Items.Clear();                                        //清空ComboBox内容
            for (int i = 1; i < 20; i++)                                //循环
            {
                try                                                     //核心原理是依靠try和catch完成遍历
                {
                    Buffer = "COM" + i.ToString();
                    MyPort.PortName = Buffer;
                    MyPort.Open();                                      //如果失败，后面的代码不会执行
                    MyBox.Items.Add(Buffer);                            //打开成功，添加至下俩列表
                    MyPort.Close();                                     //关闭
                }
                catch
                {
                }
            }
        }

        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)//串口数据接收事件
        {
            if (!radioButton3.Checked)//如果接收模式为字符模式
            {
                string str = serialPort1.ReadExisting();//字符串方式读
                textBox1.AppendText(str);//添加内容
            }
            else { //如果接收模式为数值接收
                byte data;
                data = (byte)serialPort1.ReadByte();//此处需要强制类型转换，将(int)类型数据转换为(byte类型数据，不必考虑是否会丢失数据
                string str = Convert.ToString(data, 16).ToUpper();//转换为大写十六进制字符串
                textBox1.AppendText("0x" + (str.Length == 1 ? "0" + str : str) + " ");//空位补“0”   
                //上一句等同为：if(str.Length == 1)
                //                  str = "0" + str;
                //              else 
                //                  str = str;
                //              textBox1.AppendText("0x" + str);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            byte[] Data = new byte[1];//作用同上集
            if (serialPort1.IsOpen)//判断串口是否打开，如果打开执行下一步操作
            {
                if (textBox2.Text != "")
                {
                    if (!radioButton1.Checked)//如果发送模式是字符模式
                    {
                        try
                        {
                            serialPort1.WriteLine(textBox2.Text);//写数据
                        }
                        catch (Exception err)
                        {
                            MessageBox.Show("串口数据写入错误", "错误");//出错提示
                            serialPort1.Close();
                            button1.Text = "打开端口";
                        }
                    }
                    else
                    {
                        for (int i = 0; i < (textBox2.Text.Length - textBox2.Text.Length % 2) / 2; i++)//取余运算作用是防止用户输入的字符为奇数个
                        {
                            Data[0] = Convert.ToByte(textBox2.Text.Substring(i * 2, 2), 16);
                            serialPort1.Write(Data, 0, 1);//循环发送（如果输入字符为0A0BB,则只发送0A,0B）
                        }
                        if (textBox2.Text.Length % 2 != 0)//剩下一位单独处理
                        {
                            Data[0] = Convert.ToByte(textBox2.Text.Substring(textBox2.Text.Length - 1, 1), 16);//单独发送B（0B）
                            serialPort1.Write(Data, 0, 1);//发送
                        }
                   }
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)//扫描按键
        {
            SearchAndAddSerialToComboBox(serialPort1, comboBox1);//扫描可用端口
        }
    }
}
