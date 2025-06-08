using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MainClass;

namespace RSMdlMgrClass
{
    public class RSMdlMgr
    {
        public string[] model_array = {
            "MEI_2Rail_150.smf",
            "MEI_1Rail_100.smf",
            "MEI_1Rail_50.smf",
            "MEI_1Rail_25.smf",
            "MEI_1RAIL_Bridge50_NoBanister.smf",
        };

        public string[] rail_model_array = {
            "MEI_2Rail_150.smf",
            "MEI_1Rail_100.smf",
            "MEI_1Rail_50.smf",
            "MEI_1Rail_25.smf",
            "MEI_1RAIL_Bridge50_NoBanister.smf",
        };

        public RSMdlMgr()
        {
            for (int i = 0; i < model_array.Length; i++)
            {
                model_array[i] = model_array[i].ToUpper();
            }
            for (int i = 0; i < rail_model_array.Length; i++)
            {
                rail_model_array[i] = rail_model_array[i].ToUpper();
            }
        }

        public GameObject MdlCreate(string mdl_name, string type, Main mMain)
        {
            string search_mdl = mdl_name.ToUpper();
            if (type == "rail")
            {
                if (System.Array.IndexOf(rail_model_array, search_mdl) == -1)
                {
                    mMain.DebugError("レールとして置けないモデル：" + mdl_name);
                    return null;
                }
            }
            search_mdl = search_mdl.Replace(".SMF", "");
            string path = string.Format("oldPrefab/{0}", search_mdl);
            GameObject gameObject = null;
            try
            {
                gameObject = (UnityEngine.Object.Instantiate(Resources.Load(path)) as GameObject);
                gameObject.name = gameObject.name.Replace("(Clone)", "");
            }
            catch(System.Exception e)
            {
                mMain.DebugError("MdlCreate Exception Err!!");
                mMain.DebugError(e.ToString());
            }
            return gameObject;
        }
    }
}