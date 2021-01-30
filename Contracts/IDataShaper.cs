using Entities.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace Contracts
{
    public interface IDataShaper<T>
    {
        IEnumerable<Entity> ShapeData(IEnumerable<T> entities, string fieldsString);
        Entity ShapeData(T entity, string fieldsString);
    }
}
