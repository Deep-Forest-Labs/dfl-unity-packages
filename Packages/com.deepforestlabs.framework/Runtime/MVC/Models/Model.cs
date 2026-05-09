#nullable enable
using System;
using System.Collections.Generic;
using Cysharp.Text;
using DeepForestLabs.MVC.Constants;
using UnityEngine;

namespace DeepForestLabs.MVC.Models
{
	public interface IModel
	{
		object Id { get; }
		
		string Name { get; }
	}
	
	[Serializable]
	public abstract record Model<TId> : IModel
		where TId : Enum
	{
		[SerializeField] private TId _id = default!;
		[SerializeField] private string _name = default!;

		public virtual string Name => _name;

		public TId Id => _id;
		object IModel.Id => _id;
		
		public override int GetHashCode()
		{
			return EqualityComparer<TId>.Default.GetHashCode(Id);
		}

		public virtual bool Equals(Model<TId>? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Id.Equals(other.Id);
		}

		public override string ToString()
		{
			return ZString.Format("{0}[{1}.{2} '{3}']{4} ", MVCConstants.COLOR_TAG_OPEN, typeof(TId), Id.ToString(), Name,  MVCConstants.COLOR_TAG_CLOSE);
		}
	}
}

#nullable disable
