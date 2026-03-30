using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestionAnswerHandler : MonoBehaviour
{
    public string[] GenerateOptions(string correctAnswer, string excludeComboID)
    {
        ClueDatabase db = ClueDatabaseManager.Instance.database;

        // Get 2 random incorrect answers from other combos
        List<string> incorrectAnswers = db.allCombos
            .Where(c => c.comboID != excludeComboID && c.correctAnswer != correctAnswer)
            .OrderBy(x => Random.value)
            .Take(2)
            .Select(c => c.correctAnswer)
            .ToList();

        // Combine and shuffle
        List<string> allOptions = new List<string> { correctAnswer };
        allOptions.AddRange(incorrectAnswers);

        return allOptions.OrderBy(x => Random.value).ToArray();
    }
}
