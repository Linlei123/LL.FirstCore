using LL.Core.Common.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LL.Core.Common.Helpers
{
    /// <summary>
    /// 枚举操作
    /// </summary>
    public static class EnumHelper
    {
        /// <summary>
        /// 获取实例
        /// </summary>
        /// <typeparam name="TEnum">枚举类型</typeparam>
        /// <param name="member">成员名或值,范例:Enum1枚举有成员A=0,则传入"A"或"0"获取 Enum1.A</param>
        public static TEnum Parse<TEnum>(object member)
        {
            string value = member.SafeString();
            if (string.IsNullOrWhiteSpace(value))
            {
                if (typeof(TEnum).IsGenericType)
                    return default;
                throw new ArgumentNullException(nameof(member));
            }
            return (TEnum)Enum.Parse(CommonHelper.GetType<TEnum>(), value, true);
        }

        /// <summary>
        /// 获取成员名
        /// </summary>
        /// <typeparam name="TEnum">枚举类型</typeparam>
        /// <param name="member">成员名、值、实例均可,范例:Enum1枚举有成员A=0,则传入Enum1.A或0,获取成员名"A"</param>
        public static string GetName<TEnum>(object member)
        {
            return GetName(CommonHelper.GetType<TEnum>(), member);
        }

        /// <summary>
        /// 获取成员名
        /// </summary>
        /// <param name="type">枚举类型</param>
        /// <param name="member">成员名、值、实例均可</param>
        public static string GetName(Type type, object member)
        {
            if (null == type || null == member)
                return string.Empty;
            if (member is string)
                return member.ToString();
            if (type.GetTypeInfo().IsEnum == false)
                return string.Empty;

            return Enum.GetName(type, member);
        }

        /// <summary>
        /// 获取成员值
        /// </summary>
        /// <typeparam name="TEnum">枚举类型</typeparam>
        /// <param name="member">成员名、值、实例均可，范例:Enum1枚举有成员A=0,可传入"A"、0、Enum1.A，获取值0</param>
        public static int GetValue<TEnum>(object member)
        {
            return GetValue(CommonHelper.GetType<TEnum>(), member);
        }

        /// <summary>
        /// 获取成员值
        /// </summary>
        /// <param name="type">枚举类型</param>
        /// <param name="member">成员名、值、实例均可</param>
        public static int GetValue(Type type, object member)
        {
            string value = member.SafeString();
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(member));
            return (int)Enum.Parse(type, member.ToString(), true);
        }

        /// <summary>
        /// 获取名称集合
        /// </summary>
        /// <typeparam name="TEnum">枚举类型</typeparam>
        public static List<string> GetNames<TEnum>()
        {
            return GetNames(typeof(TEnum));
        }

        /// <summary>
        /// 获取名称集合
        /// </summary>
        /// <param name="type">枚举类型</param>
        public static List<string> GetNames(Type type)
        {
            type = CommonHelper.GetType(type);
            if (!type.IsEnum)
                throw new InvalidOperationException($"类型 {type} 不是枚举");
            var result = new List<string>();
            foreach (var field in type.GetFields())
            {
                if (!field.FieldType.IsEnum)
                    continue;
                result.Add(field.Name);
            }
            return result;
        }

        /// <summary>
        /// 获取描述,使用System.ComponentModel.Description特性设置描述
        /// </summary>
        /// <param name="type">枚举类型</param>
        /// <param name="member">成员名、值、实例均可</param>
        public static string GetDescription(Type type, object member)
        {
            return GetDescription(type, GetName(type, member));
        }

        /// <summary>
        /// 获取类型成员描述，使用DescriptionAttribute设置描述
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="memberName">成员名称</param>
        public static string GetDescription(Type type, string memberName)
        {
            if (type == null || string.IsNullOrWhiteSpace(memberName))
                return string.Empty;

            var member = type.GetTypeInfo().GetMember(memberName).FirstOrDefault();
            if (member == null)
                return string.Empty;

            return member.GetCustomAttribute<DescriptionAttribute>() is DescriptionAttribute attribute ? attribute.Description : member.Name;
        }
    }
}
