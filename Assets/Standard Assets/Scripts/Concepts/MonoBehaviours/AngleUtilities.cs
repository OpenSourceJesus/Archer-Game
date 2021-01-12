namespace Extensions
{
	public static class AngleUtilites
	{
		public static AngleRange RangeFromMinAndMax (float minDegrees, float maxDegrees)
		{
			return new AngleRange(new Angle(minDegrees), new Angle(maxDegrees));
		}
	}
}