using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace gsi
{
    class JsonComparator
    {
        private JObject obj1;
        private JObject obj2;
        private List<string> excludedAttributes;
        private List<string> includedAttributes;
        private bool ignoreAppended;

        public JsonComparator(string text1, string text2, List<string> exclude, List<string> include, bool ignore_append)
        {
            this.obj1 = JObject.Parse(text1);
            this.obj2 = JObject.Parse(text2);

            this.excludedAttributes = exclude;
            this.includedAttributes = include;
            this.ignoreAppended = ignore_append;
        }

        /*
        {key:value}
        value={change_key:{key:???}}
        */
        private bool IsIncexKey(string key, JToken value)
        {
            bool keyOut = ((this.includedAttributes.Count > 0 && !this.includedAttributes.Contains(key)) ||
                        this.excludedAttributes.Contains(key));

            bool valueOut = true;
            if (value is JObject value_Object)
                foreach (var changeKeyValue in value_Object)
                    if (changeKeyValue.Value is JObject changeKeyValue_Value_Object)
                        foreach (var nestedKeyValue in changeKeyValue_Value_Object)
                            if ((this.includedAttributes.Count > 0 && this.includedAttributes.Contains(nestedKeyValue.Key)) ||
                                !this.excludedAttributes.Contains(nestedKeyValue.Key))
                            {
                                valueOut = false;
                            }

            return keyOut && valueOut;
        }

        private JObject FilterResults(JObject result)
        {
            JObject outResult = new JObject();

            foreach (var changeKeyValue in result)
            {
                JObject tempDict = new JObject();
                if (changeKeyValue.Value is JObject changeKeyValue_Value_Object)
                {
                    foreach (var pairKeyValue in changeKeyValue_Value_Object)
                    {
                        if (ignoreAppended && changeKeyValue.Key == "_append")
                            continue;
                        if (!IsIncexKey(pairKeyValue.Key, pairKeyValue.Value))
                            tempDict[pairKeyValue.Key] = pairKeyValue.Value;
                    }
                    if (tempDict.Count > 0)
                        outResult[changeKeyValue.Key] = tempDict;
                }
                else
                {
                    throw new Exception($"{changeKeyValue.Value} is not json object");
                }
            }

            return outResult;
        }

        private JToken CompareElements(JToken oldToken, JToken newToken)
        {
            JToken res = null;

            if (oldToken is JObject)
            {
                JObject resDict = CompareDicts(oldToken as JObject, newToken as JObject);
                if (resDict.Count > 0)
                    res = resDict;
            }
            else if (oldToken.GetType() != newToken.GetType())
            {
                res = newToken;
            }
            else if (oldToken is JArray)
            {
                JObject resArr = CompareArrays(oldToken as JArray, newToken as JArray);
                if (resArr.Count > 0)
                    res = resArr;
            }
            else
            {
                JValue scalarDiff = CompareScalars(oldToken as JValue, newToken as JValue);
                if (scalarDiff != null)
                    res = scalarDiff;
            }

            return res;
        }

        private JValue CompareScalars(JValue oldToken, JValue newToken)
        {
            if (!oldToken.Equals(newToken))
                return newToken;
            else
                return null;
        }

        private JObject CompareArrays(JArray oldArr, JArray newArr)
        {
            int inters = Math.Min(oldArr.Count, newArr.Count);

            JObject result = new JObject();
            JObject appendObj = new JObject();
            JObject removeObj = new JObject();
            JObject updateObj = new JObject();

            for (int idx = 0; idx < inters; idx++)
            {
                JToken res = CompareElements(oldArr[idx], newArr[idx]);
                if (res != null)
                    updateObj[idx.ToString()] = res;
            }

            if (inters == oldArr.Count)
            {
                for (int idx = inters; idx < newArr.Count; idx++)
                    appendObj[idx.ToString()] = newArr[idx];
            }
            else
            {
                for (int idx = inters; idx < oldArr.Count; idx++)
                    removeObj[idx.ToString()] = oldArr[idx];
            }

            if (appendObj.Count > 0)
                result["_append"] = appendObj;
            if (removeObj.Count > 0)
                result["_remove"] = removeObj;
            if (updateObj.Count > 0)
                result["_update"] = updateObj;

            return FilterResults(result);
        }

        public JObject CompareDicts(JObject oldObj = null, JObject newObj = null)
        {
            if (oldObj == null && obj1 != null)
                oldObj = obj1;
            if (newObj == null && obj2 != null)
                newObj = obj2;

            var oldKeys = new HashSet<string>(oldObj?.Properties().Select(p => p.Name) ?? Enumerable.Empty<string>());
            var newKeys = new HashSet<string>(newObj?.Properties().Select(p => p.Name) ?? Enumerable.Empty<string>());
            var keys = oldKeys.Union(newKeys);

            var result = new JObject();
            result["_append"] = new JObject();
            result["_remove"] = new JObject();
            result["_update"] = new JObject();
            foreach (var name in keys)
            {
                if (!oldKeys.Contains(name))
                {
                    result["_append"][name] = newObj[name];
                }
                else if (!newKeys.Contains(name))
                {
                    result["_remove"][name] = oldObj[name];
                }
                else
                {
                    var res = CompareElements(oldObj[name], newObj[name]);
                    if (res != null)
                    {
                        result["_update"][name] = JToken.FromObject(res);
                    }
                }
            }

            return FilterResults(result);
        }
    }
}


