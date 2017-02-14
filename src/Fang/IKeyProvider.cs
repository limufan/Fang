using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fang
{
    public interface IIdProvider
    {
        int ID { get; }
    }

    public interface ICodeProvider
    {
        string Code { get; }
    }

    public interface IGuidProvider
    {
        string Guid { get; }
    }

    public interface IIdCodeProvider : ICodeProvider, IIdProvider
    {

    }

    public interface IGuidCodeProvider : ICodeProvider, IGuidProvider
    {

    }

    public interface IIdGuidProvider : IGuidProvider, IIdProvider
    {

    }

    public interface IIdCodeNameProvider : IIdCodeProvider
    {
        string UniqueName { get; }
    }

    public interface IIdCodeLongCodeNameProvider : IIdCodeLongCodeProvider
    {
        string UniqueName { get; }
    }

    public interface IIdCodeLongCodeProvider : IIdCodeProvider
    {
        string LongCode { get; }
    }

}
