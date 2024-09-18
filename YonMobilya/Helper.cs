using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;

namespace YonMobilya
{
    static class Helper
    {
        #region DataTableToClassList

        public static List<T> DataTableToList<T>(this DataTable table) where T : class, new()

        {

            try

            {

                List<T> list = new List<T>();



                foreach (var row in table.AsEnumerable())

                {

                    T obj = new T();



                    foreach (var prop in obj.GetType().GetProperties())

                    {

                        try

                        {

                            PropertyInfo propertyInfo = obj.GetType().GetProperty(prop.Name);

                            propertyInfo.SetValue(obj, Convert.ChangeType(row[prop.Name], GetCoreType(propertyInfo.PropertyType)), null);

                            //propertyInfo.SetValue(obj, Convert.ChangeType(row[prop.Name], propertyInfo.PropertyType), null);

                        }

                        catch

                        {

                            continue;

                        }

                    }



                    list.Add(obj);

                }



                return list;

            }

            catch

            {

                return null;

            }

        }



        private static Type GetCoreType(Type type)

        {

            if (type.IsGenericType &&

                type.GetGenericTypeDefinition() == typeof(Nullable<>))

                return Nullable.GetUnderlyingType(type);

            else

                return type;

        }



        #endregion



        #region Others



        private static T ChangeType<T>(object value)

        {

            var t = typeof(T);



            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))

            {

                if (value == null)

                {

                    return default(T);

                }



                t = Nullable.GetUnderlyingType(t);

            }



            return (T)Convert.ChangeType(value, t);

        }



        private static object ChangeType(object value, Type conversion)

        {

            var t = conversion;



            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))

            {

                if (value == null)

                {

                    return null;

                }



                t = Nullable.GetUnderlyingType(t);

            }



            return Convert.ChangeType(value, t);

        }



        #endregion
    }
}