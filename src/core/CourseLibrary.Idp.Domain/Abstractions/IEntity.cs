using System;
using System.Collections.Generic;
using System.Text;

namespace CourseLibrary.Idp.Domain.Abstractions;

public interface IEntity
{

}

public interface IEntity<TId> : IEntity
{
    TId Id { get; set; }
}
