using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * AttributeComponent
 * 
 * Component for holding a named list of attributes, where an attribute is a value comprised
 * of multiple contributions.
 */
public class AttributesContainer
{
	private Dictionary<string, Attribute> _contributionMap;

	public AttributesContainer()
	{
		_contributionMap = new Dictionary<string, Attribute>();
	}
	
	/** AddContribution - add a contribution to the given attribute. */
	public void AddContribution(string attribute, string contributionCategory, AttributeContributionType contributionType, float value)
	{
		if (!_contributionMap.ContainsKey(attribute))
		{
			_contributionMap[attribute] = new Attribute();
		}
		_contributionMap[attribute].AddContribution(contributionCategory, contributionType, value);
	}

	public void AddContribution(string attribute, string contributionCategory, AttributeContributionType contributionType, int value)
	{
		AddContribution(attribute, contributionCategory, contributionType, 1.0f * value);
	}

	/** RemoveContribution - remove all contributions from the given attribute that match the given contributionCategory. */
	public void RemoveContribution(string attribute, string contributionCategory)
	{
		if (_contributionMap.ContainsKey(attribute))
		{
			_contributionMap[attribute].RemoveContribution(contributionCategory);
		}
	}

	/** GetValue - given an attribute name, look for it in the map and return its value if it exists, otherwise return 0. */
	public float GetValue(string attribute, float defaultValue = 0.0f)
	{
		return _contributionMap.ContainsKey(attribute) ? _contributionMap[attribute].GetValue() : defaultValue;
	}

	public int GetValueAsInt(string attribute, int defaultValue = 0)
	{
		return _contributionMap.ContainsKey(attribute) ? Mathf.RoundToInt(_contributionMap[attribute].GetValue()) : defaultValue;
	}

}

/**
 * Attribute - atomic instance for a stat value held by the attributes component.
 * Receives contributions from sources and keeps track of a cached resulting value
 * as contributions are added and removed.
 */
public class Attribute
{
	private List<AttributeContribution> _contributionList;
	private float _cachedAdds = 0.0f;
	private float _cachedMultiplies = 1.0f;
	private float _cachedOverride = 0.0f;
	
	public Attribute()
	{
		_contributionList = new List<AttributeContribution>();
	}

	/** AddContribution - Adds a contribution with the given category as an identifier. */
	public void AddContribution(string contributionCategory, AttributeContributionType contributionType, float value)
	{
		AttributeContribution contribution = new AttributeContribution();
		contribution.ContributionCategory = contributionCategory;
		contribution.ContributeType = contributionType;
		contribution.Value = value;

		_contributionList.Add(contribution);

		switch (contributionType)
		{
			case AttributeContributionType.Additive:
				{
					_cachedAdds += value;
					break;
				}
			case AttributeContributionType.Multiply:
				{
					_cachedMultiplies *= value;
					break;
				}
			case AttributeContributionType.Override:
				{
					_cachedOverride = value;
					break;
				}
		}
	}

	/** RemoveContribution - Removes all contributions that match the given category. */
	public void RemoveContribution(string contributionCategory)
	{
		List<AttributeContribution> matches = _contributionList.FindAll(x => x.ContributionCategory.Equals(contributionCategory));
		foreach (AttributeContribution match in matches)
		{
			switch (match.ContributeType)
			{
				case AttributeContributionType.Additive:
					{
						_cachedAdds -= match.Value;
						break;
					}
				case AttributeContributionType.Multiply:
					{
						_cachedMultiplies /= match.Value;
						break;
					}
				case AttributeContributionType.Override:
					{
						_cachedOverride = 0.0f;
						break;
					}
			}
			_contributionList.Remove(match);
		}
	}

	/** GetValue - Return the net result of this attribute, returns the overriding attribute if it has been set, otherwise returns [sum of additive contributions] x [product of multiply contributions]. */
	public float GetValue()
	{
		return _cachedOverride != 0.0f ? _cachedOverride : _cachedAdds * _cachedMultiplies;
	}
}

/** Different types of attribute contributions - determines how they get added to the final result of an attribute. */
public enum AttributeContributionType
{
	Additive, // = Is added to the base value first.
	Multiply, // = Multiplies the net result (after all additives have been combined).
	Override, // = Ignores all other contributions and becomes the final result. It is assumed that attributes will never have more than 1 override attribute.
}

/** AttributeContribution - Data class holding contribution info to be stored in an attribute. */
public class AttributeContribution
{
	public string ContributionCategory { get; set; }
	public AttributeContributionType ContributeType { get; set; }
	public float Value { get; set; }
}

/** Struct to be used for user-defined attribute contributions in editor. */
[System.Serializable]
public struct AttributeData
{
	[SerializeField] public string Attribute;
	[SerializeField] public string Category;
	[SerializeField] public AttributeContributionType ContributionType;
	[SerializeField] public float Value;
}
