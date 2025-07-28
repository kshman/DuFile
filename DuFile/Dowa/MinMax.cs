using System.Diagnostics.CodeAnalysis;
// ReSharper disable UnassignedReadonlyField

namespace DuFile.Dowa;

/// <summary>
/// 최소값과 최대값을 나타내는 구조체입니다.
/// </summary>
[Serializable]
public struct MinMax : IEquatable<MinMax>
{
	/// <summary>
	/// 비어있는 <see cref="MinMax"/> 구조체를 나타냅니다.
	/// </summary>
	public static readonly MinMax Empty;

	private int min;
	private int max;

	/// <summary>
	/// 지정한 최소값과 최대값으로 <see cref="MinMax"/> 구조체를 초기화합니다.
	/// </summary>
	/// <param name="min">최소값</param>
	/// <param name="max">최대값</param>
	public MinMax(int min, int max)
	{
		if (min > max)
		{
			this.min = max;
			this.max = min;
		}
		else
		{
			this.min = min;
			this.max = max;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="MinMax"/> class, calculating the minimum and maximum values from the
	/// specified collection of integers.
	/// </summary>
	/// <param name="values">A collection of integers from which to determine the minimum and maximum values. This collection must not be null
	/// or empty.</param>
	public MinMax(IEnumerable<int> values)
	{
		min = int.MaxValue;
		max = int.MinValue;
		foreach (var value in values)
		{
			if (value < min) min = value;
			if (value > max) max = value;
		}
	}

	/// <summary>
	/// <see cref="MinMax"/>가 비어있는지 여부를 반환합니다.
	/// </summary>
	[Browsable(false)]
	public readonly bool IsEmpty => min == 0 && max == 0;

	/// <summary>
	/// 최소값을 가져오거나 설정합니다.
	/// </summary>
	public int Min
	{
		readonly get => min;
		set => SetMin(value);
	}

	/// <summary>
	/// 최대값을 가져오거나 설정합니다.
	/// </summary>
	public int Max
	{
		readonly get => max;
		set => SetMax(value);
	}

	/// <summary>
	/// Gets or sets the minimum value for the range.
	/// </summary>
	public int Left
	{
		readonly get => min;
		set => SetMin(value);
	}

	/// <summary>
	/// Gets or sets the maximum value for the right boundary.
	/// </summary>
	public int Right
	{
		readonly get => max;
		set => SetMax(value);
	}

	/// <summary>
	/// Gets the difference between the maximum and minimum values.
	/// </summary>
	public int Range => Max - Min;

	/// <summary>
	/// Gets the length of the range.
	/// </summary>
	public int Length => Range;

	/// <summary>
	/// 두 <see cref="MinMax"/> 구조체가 같은지 비교합니다.
	/// </summary>
	/// <param name="left">왼쪽 피연산자</param>
	/// <param name="right">오른쪽 피연산자</param>
	/// <returns>두 값이 같으면 true, 아니면 false</returns>
	public static bool operator ==(MinMax left, MinMax right) =>
		left.Min == right.Min && left.Max == right.Max;

	/// <summary>
	/// 두 <see cref="MinMax"/> 구조체가 다른지 비교합니다.
	/// </summary>
	/// <param name="left">왼쪽 피연산자</param>
	/// <param name="right">오른쪽 피연산자</param>
	/// <returns>두 값이 다르면 true, 아니면 false</returns>
	public static bool operator !=(MinMax left, MinMax right) =>
		!(left == right);

	/// <inheritdoc/>
	public readonly override bool Equals([NotNullWhen(true)] object? obj) =>
		obj is MinMax mm && Equals(mm);

	/// <inheritdoc/>
	public readonly bool Equals(MinMax other) => this == other;

	/// <inheritdoc/>
	public readonly override int GetHashCode() => HashCode.Combine(Min, Max);

	/// <summary>
	/// 최소값과 최대값에 지정한 값을 더합니다.
	/// </summary>
	/// <param name="dmin">더할 최소값</param>
	/// <param name="dmax">더할 최대값</param>
	public void Offset(int dmin, int dmax)
	{
		unchecked
		{
			min += dmin;
			max += dmax;
			Validate();
		}
	}

	/// <summary>
	/// 다른 <see cref="MinMax"/>의 값만큼 최소값과 최대값을 더합니다.
	/// </summary>
	/// <param name="p">더할 <see cref="MinMax"/> 값</param>
	public void Offset(MinMax p) => Offset(p.Min, p.Max);

	/// <summary>
	/// Determines whether the specified value is within the defined range.
	/// </summary>
	/// <param name="value">The integer value to check against the range.</param>
	/// <returns><see langword="true"/> if the specified value is greater than or equal to the minimum and less than or equal to the
	/// maximum; otherwise, <see langword="false"/>.</returns>
	public bool Contains(int value) =>
		value >= Min && value <= Max;

	/// <inheritdoc/>
	public readonly override string ToString() =>
		$"{{작은값={Min},큰값={Max}}}";

	private void Validate()
	{
		if (min > max)
			(min, max) = (max, min);
	}

	private void SetMin(int value)
	{
		min = value;
		Validate();
	}

	private void SetMax(int value)
	{
		max = value;
		Validate();
	}
}
