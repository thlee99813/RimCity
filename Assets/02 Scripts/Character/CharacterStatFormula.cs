using UnityEngine;

public static class CharacterStatFormula
{
    public static float CalculateMaxHealthByAge(int age)
    {
        age = Mathf.Clamp(age, 0, 100);

        if (age <= 35)
        {
            float t = age / 35f;
            return Mathf.Lerp(50f, 100f, t);
        }

        float t2 = (age - 35f) / 65f;
        return Mathf.Lerp(100f, 10f, t2);
    }
}
