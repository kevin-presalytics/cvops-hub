using System.ComponentModel.DataAnnotations;
using System;

namespace lib.models
{
    public interface IBaseEntity
    {
        Guid Id {get; set;}
        DateTime DateCreated {get; set;}
        Guid? UserCreated {get; set;}
        DateTime DateModified {get; set;}
        Guid? UserModified {get;set;}
        EditorTypes CreatedBy {get; set;} // System or User
        EditorTypes ModifiedBy {get; set;} // System or User
    }


    public abstract class BaseEntity : IBaseEntity
    {
        [Key]
        public Guid Id {get; set;}
        public DateTime DateCreated {get; set;}
        public Guid? UserCreated {get; set;}
        public EditorTypes CreatedBy {get; set;} // System or User
        public DateTime DateModified {get; set;}
        public Guid? UserModified {get;set;}
        public EditorTypes ModifiedBy {get; set;} // System or User
    }

    public enum EditorTypes
    {
        System,
        Device,
        User
    }
    
}