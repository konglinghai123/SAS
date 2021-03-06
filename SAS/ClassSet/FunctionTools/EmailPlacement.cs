﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SAS.ClassSet.Common;
using SAS.ClassSet.FunctionTools;
using SAS.ClassSet.MemberInfo;
using System.Windows.Forms;
using System.Data;
namespace SAS.ClassSet.FunctionTools
{
    class EmailPlacement
    {
        ListView listView1;
        private int recnuum=0;
        private int sentnum=0;
        private int successnum=0;
        public EmailPlacement(ListView listView1)
        {
            this.listView1 = listView1;
        }
        private WordTableInfo InitializeWordInfo()
        {
            WordTableInfo Info = new WordTableInfo();
            Info.Supervisor = listView1.CheckedItems[0].SubItems[9].Text;
            Info.Time = listView1.CheckedItems[0].SubItems[8].Text;
            Info.Classroom = listView1.CheckedItems[0].SubItems[5].Text;
            Info.Perfession = listView1.CheckedItems[0].SubItems[4].Text;
            Info.Teacher = listView1.CheckedItems[0].SubItems[6].Text;
            Info.Class = listView1.CheckedItems[0].SubItems[1].Text;
            Info.Subject = listView1.CheckedItems[0].SubItems[2].Text;
            Info.Teachingtype = listView1.CheckedItems[0].SubItems[3].Text;
            return Info;

        }
        private void FillWordTable(List<string> ListSupervisor,WordTableInfo Info,WordTools Tool,List<string> ListFileName)
        {
            for (int i = 0; i < ListSupervisor.Count; i++)
            {
                if (i == 0)
                {
                    Info.Supervisor = ListSupervisor[0];
                   // ListFileName.Add(Tool.Addchiefsupervisordata(Info));
                    ListFileName.Add(Tool.fullcheifsupervisor(Info));
                }
                else
                {
                    Info.Supervisor = ListSupervisor[i];
                   // ListFileName.Add(Tool.Addsupervisordata(Info));
                    ListFileName.Add(Tool.fullsupervisor(Info));
                }
            }
        }
        private  void FindEmailAddress(List<string> ListSupervisor,SqlHelper help,List<string> ListAddress){
              for (int i = 0; i < ListSupervisor.Count; i++)
            {
                string selectcommand = "select * from Teachers_Data where Teacher like '"+"%" + ListSupervisor[i] +"%"+ "'";
                DataRow[] dr = help.getDs(selectcommand, "Teachers_Data").Tables[0].Select();
                ListAddress.Add(dr[0][2].ToString());
            }
        }
        private void MakeEmail(EmailInfo EInfo,List<string> ListFileName,List<string> ListAddress,SqlHelper help,List<string> ListSupervisor){
            EInfo.User = Common.Common.MailAddress;
            EInfo.PassWord = Common.Common.MailPassword;
            EmailRecordInfo ERecord;
            sentnum = ListFileName.Count;
            for (int i = 0; i < ListFileName.Count; i++)
            {
                EInfo.AddFiles = ListFileName[i];
                EInfo.Content = "";
                EInfo.Receiver = ListAddress[i];
                EInfo.Title = DateTime.Now + "听课安排";
                string successflag="";
                ERecord = new EmailRecordInfo(ListSupervisor[i], "督导", EInfo.Title, ListSupervisor[i] + DateTime.Now.ToLongTimeString()+i, "听课安排", successflag, ListFileName[i]);

                AsynEmail EmailSendPoccess = new AsynEmail(EInfo, ERecord, this.EmailResultCallBack);
                EmailSendPoccess.ThreadSend();
                //MessageBox.Show(successflag);
                //help.Insert(ERecord,"Logs_Data");
                Main.fm.SetStatusText("正在发送邮件", 0);
            }

        }
        private void EmailResultCallBack(EmailRecordInfo info, string message)
        {
            recnuum++;
            Main.fm.SetStatusText(string.Format("已发送{0}封",recnuum), 0);
            SqlHelper help = new SqlHelper();
            help.Insert(info, "Logs_Data");
            if (message=="发送成功")
            {   
                successnum++;
            }
            if(recnuum==sentnum){
                
                MessageBox.Show(string.Format("共发送{0}邮件,成功{1}封，失败{2}封，请查看记录",sentnum,successnum,sentnum-successnum));
                Main.fm.SetStatusText("发送完成", 0);
            }
           
            
        }
        public void SentPlacement()
        {
            WordTableInfo Info = InitializeWordInfo();//对象初始化
            WordTools Tool = new WordTools();
            List<string> ListSupervisor = new List<string>();//存放督导成员
            List<string> ListFileName = new List<string>();
          
            EmailInfo EInfo = new EmailInfo();
            SqlHelper help = new SqlHelper();
            List<string> ListAddress = new List<string>();
            DistinctSupervisor(Info.Supervisor, ListSupervisor);//分解出每个督导员
            FillWordTable(ListSupervisor,Info,Tool,ListFileName);//填写相应的word表格
            FindEmailAddress(ListSupervisor,help,ListAddress);//找到每个人的邮箱地址
            MakeEmail(EInfo,ListFileName,ListAddress,help,ListSupervisor);//发邮件
          
          
        }

        private string DistinctSupervisor(string supervisor, List<string> ListSupervisor)
        {
            if (supervisor.IndexOf(",") != -1)
            {
                ListSupervisor.Add(supervisor.Substring(0, supervisor.IndexOf(",")));
                return DistinctSupervisor(supervisor.Substring(supervisor.IndexOf(",") + 1), ListSupervisor);
            }
            else
            {
                ListSupervisor.Add(supervisor);
                return supervisor;
            }

        }
      
    }
}
