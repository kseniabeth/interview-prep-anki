using System;

decimal price = 19.95m;
decimal discounted = decimal.Round(price * 0.9m, 2, MidpointRounding.ToEven);
Console.WriteLine(discounted.ToString(System.Globalization.CultureInfo.InvariantCulture)); // 17.96
long total = checked((long)int.MaxValue + 1);
Console.WriteLine(total); // 2147483648
