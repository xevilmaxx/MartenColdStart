using Baseline.Reflection;
using LibRtDb.DTO.DeviceConfigs;
using Marten;
//using Marten.Linq.Fields;
//using Marten.Linq.Filters;
//using Marten.Linq.Parsing;
//using Marten.Linq.SqlGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// WORKS in MARTEN >= V4, for now for stability retreated to MARTEN V3
/// </summary>
namespace LibRtDb.Extensions.Linq.MyEquals
{
    //public class LinqEqualsExt : IMethodCallParser
    //{
    //    /// <summary>
    //    /// Hook to a static function
    //    /// </summary>
    //    /// <param name="expression"></param>
    //    /// <returns></returns>
    //    public bool Matches(MethodCallExpression expression)
    //    {
    //        return expression.Method.Name == nameof(CustomExtensions.MyEquals);
    //    }

    //    /// <summary>
    //    /// Mainly it overrides Original Method defined in CustomExtensions.MyEquals
    //    /// </summary>
    //    /// <param name="mapping"></param>
    //    /// <param name="serializer"></param>
    //    /// <param name="expression"></param>
    //    /// <returns></returns>
    //    public ISqlFragment Parse(IFieldMapping mapping, ISerializer serializer, MethodCallExpression expression)
    //    {
    //        //get object witch which we want to compare
    //        var compareTo = expression.Arguments[1].Value();

    //        //get all of its properties
    //        PropertyInfo[] _properties = compareTo.GetType().GetProperties();

    //        string addedSql = "";
    //        var lastProp = _properties.Last();

    //        for (int i = 0; i < _properties.Length; i++)
    //        {
    //            var locator = mapping.FieldFor(new MemberInfo[] { _properties[i] }).TypedLocator;
    //            if (i == (_properties.Length - 1))
    //            {
    //                //last element of an object
    //                addedSql += $"{locator} = '{_properties[i].GetValue(compareTo)}'";
    //            }
    //            else
    //            {
    //                addedSql += $"{locator} = '{_properties[i].GetValue(compareTo)}' and ";
    //            }
    //        }

    //        return new WhereFragment(addedSql);
    //    }

    //}
}
