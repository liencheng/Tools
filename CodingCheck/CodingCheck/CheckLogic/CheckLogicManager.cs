using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodingCheck.CheckLogic
{
    public enum CheckTypeEnum
    {
        C_Server_GetScene = 0,
        C_Server_GetXXXByIdCheck = 1,
        C_Server_ZeroCheck = 2,
        C_Server_MapErase, 
        C_Client_InstanceCheck ,
        C_Client_PublicMonoScript,
        C_Client_CoroutineRefNull,
        C_Max
    }
    public class CheckLogicManager
    {
        private Dictionary<CheckTypeEnum, CheckLogicBase> m_CheckLogic = new Dictionary<CheckTypeEnum, CheckLogicBase>();
        private CheckLogicManager() { RegisterLogic(); }
        private static CheckLogicManager m_Instance = new CheckLogicManager();
        public static CheckLogicManager Instance
        {
            get
            {
                if (null == m_Instance)
                {
                    m_Instance = new CheckLogicManager();
                }
                return m_Instance;
            }
        }
        void RegisterLogic()
        {
            m_CheckLogic[CheckTypeEnum.C_Server_GetScene] = new CheckLogic_GetScene();
            m_CheckLogic[CheckTypeEnum.C_Server_GetXXXByIdCheck] = new CheckLogic_Server_GetXXXById();
            m_CheckLogic[CheckTypeEnum.C_Server_ZeroCheck] = new CheckLogic_Server_ZeroCheck();
            m_CheckLogic[CheckTypeEnum.C_Server_MapErase] = new CheckLogic_Server_MapErase();
            m_CheckLogic[CheckTypeEnum.C_Client_InstanceCheck] = new CheckLogic_Client_InstanceCheck();
            m_CheckLogic[CheckTypeEnum.C_Client_PublicMonoScript] = new CheckLogic_Client_PublicMonoScript();
            m_CheckLogic[CheckTypeEnum.C_Client_CoroutineRefNull] = new CheckLogic_Client_CoroutineRefNull();
        }
        public CheckLogicBase GetCheckLogic(CheckTypeEnum check)
        {
            if(m_CheckLogic.ContainsKey(check))
            {
                return m_CheckLogic[check];
            }
            return null; 
        }
        
    }
}
