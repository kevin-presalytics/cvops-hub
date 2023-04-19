using System.ComponentModel.DataAnnotations;
using System;

namespace lib.models
{
    public interface IBaseEntity
    {
        Guid Id {get; set;}
        DateTime DateCreated {get; set;}
        Guid UserCreated {get; set;}
        DateTime DateModified {get; set;}
        Guid UserModified {get;set;}
    }


    public abstract class BaseEntity : IBaseEntity
    {
        [Key]
        public Guid Id {get; set;}
        public DateTime DateCreated {get; set;}
        public Guid UserCreated {get; set;}
        public DateTime DateModified {get; set;}
        public Guid UserModified {get;set;}
    }
    
}