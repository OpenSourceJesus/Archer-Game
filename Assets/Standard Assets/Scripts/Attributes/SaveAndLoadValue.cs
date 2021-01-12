using System;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class SaveAndLoadValue : Attribute
{
	public bool isShared;

	public SaveAndLoadValue (bool isShared)
	{
		this.isShared = isShared;
	}
}
