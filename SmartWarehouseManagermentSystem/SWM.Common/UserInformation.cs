using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWM.Common
{
    public class UserInformation
    {
        private string m_UserName;
        private string m_Password;
        private string m_TenNguoiDung;
        private string m_Role;
        private string m_Position;
        private string m_Email;
        private string m_EmailPassWord;
        private string m_PhoneNumber;
        private string m_WorkAddress;
        private string m_Creator;
        private string m_EmployeeID;
        private bool m_Active;

        public string UserName
        {
            get
            {
                return m_UserName;
            }

            set
            {
                m_UserName = value;
            }
        }

        public string Password
        {
            get
            {
                return m_Password;
            }

            set
            {
                m_Password = value;
            }
        }

        public string TenNguoiDung
        {
            get
            {
                return m_TenNguoiDung;
            }

            set
            {
                m_TenNguoiDung = value;
            }
        }

        public string Role
        {
            get
            {
                return m_Role;
            }

            set
            {
                m_Role = value;
            }
        }

        public bool Active
        {
            get
            {
                return m_Active;
            }

            set
            {
                m_Active = value;
            }
        }

        public string Position
        {
            get
            {
                return m_Position;
            }

            set
            {
                m_Position = value;
            }
        }

        public string Email
        {
            get
            {
                return m_Email;
            }

            set
            {
                m_Email = value;
            }
        }

        public string EmailPassWord
        {
            get
            {
                return m_EmailPassWord;
            }

            set
            {
                m_EmailPassWord = value;
            }
        }

        public string PhoneNumber
        {
            get
            {
                return m_PhoneNumber;
            }

            set
            {
                m_PhoneNumber = value;
            }
        }

        public string WorkAddress
        {
            get
            {
                return m_WorkAddress;
            }

            set
            {
                m_WorkAddress = value;
            }
        }

        public string Creator
        {
            get
            {
                return m_Creator;
            }

            set
            {
                m_Creator = value;
            }
        }

        public string EmployeeID
        {
            get
            {
                return m_EmployeeID;
            }

            set
            {
                m_EmployeeID = value;
            }
        }
    }
}
