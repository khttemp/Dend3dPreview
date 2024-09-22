using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MainClass;

namespace RSMdlMgrClass
{
    public class RSMdlMgr
    {
        Dictionary<string, string> origin_rs_rail_model_map = new Dictionary<string, string>(){
            {"MEI_2Rail_150.smf", "2Rail_Den_100"},
            {"MEI_1Rail_100.smf", "1Rail_Den_100"},
            {"MEI_1Rail_50.smf", "1Rail_Den_50"},
            {"MEI_1Rail_25.smf", "1Rail_Den_25"},
            {"MEI_1RAIL_Bridge50_NoBanister.smf", "1Rail_50_Only"}
        };
        List<string> origin_map_key_list;
        Dictionary<string, float> origin_rs_rescale_map = new Dictionary<string, float>(){
            {"MEI_2Rail_150.smf", 1.5f}
        };

        public List<string> rs_map_key_list = new List<string>();
        public Dictionary<string, string> rs_rail_model_map = new Dictionary<string, string>();
        public Dictionary<string, float> rs_rescale_map = new Dictionary<string, float>();

        public bool isEmpty;

        public RSMdlMgr()
        {
            isEmpty = false;
            origin_map_key_list = new List<string>(origin_rs_rail_model_map.Keys);
            for (int i = 0; i < origin_map_key_list.Count; i++)
            {
                string key = origin_map_key_list[i];
                rs_map_key_list.Add(key.ToUpper());
                rs_rail_model_map.Add(key.ToUpper(), origin_rs_rail_model_map[key].ToUpper());
                if (origin_rs_rescale_map.ContainsKey(key))
                {
                    rs_rescale_map.Add(key.ToUpper(), origin_rs_rescale_map[key]);
                }
            }
        }

        public GameObject MdlCreate(string mdl_name, string type, Main mMain)
        {
            string search_mdl = mdl_name.ToUpper();
            if (!rs_map_key_list.Contains(search_mdl))
            {
                if (!isEmpty)
                {
                    mMain.DebugWarning("代わりのモデルがないモデル：" + mdl_name);
                    isEmpty = true;
                }
                GameObject emptyObj = new GameObject();
                emptyObj.name = "EmptyObj";
                emptyObj.SetActive(false);
                return emptyObj;
            }
            string alter_mdl = rs_rail_model_map[search_mdl];
            string path = string.Format("prefab/{0}", alter_mdl);
            GameObject gameObject = null;
            try
            {
                gameObject = (UnityEngine.Object.Instantiate(Resources.Load(path)) as GameObject);
                gameObject.name = gameObject.name.Replace("(Clone)", "");
                if (rs_rescale_map.ContainsKey(search_mdl))
                {
                    gameObject.transform.localScale = new Vector3(1f, 1f, rs_rescale_map[search_mdl]);
                }
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