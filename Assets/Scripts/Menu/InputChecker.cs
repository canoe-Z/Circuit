using UnityEngine;
using UnityEngine.UI;

public class InputChecker : MonoBehaviour
{
	public InputField inputField;
	private bool hasScale = false;
	private bool isWrongInput = false;

	public void InputR()
	{
		char[] charArray = inputField.text.ToCharArray();

		for (int i = 0; i < charArray.Length; i++)
		{
			char ch = charArray[i];

			if (ch != 'k' && ch != 'K' && ch != 'm' && ch != 'M' && !char.IsDigit(ch))
			{
				// 千，兆两个单位和数字以外的输入非法
				isWrongInput = true;
			}
			// 合法输入
			else
			{
				// 对于单位输入
				if (!char.IsDigit(ch))
				{
					// 只接受1个单位
					if (!hasScale)
					{
						hasScale = true;
					}
					else
					{
						isWrongInput = true;
					}

					// 不接受头一个字符为单位
					if (charArray.Length == 1)
					{
						isWrongInput = true;
					}
				}

				// 对于数字输入
				else
				{
					// 不接受在单位后输入数字
					if (hasScale)
					{
						isWrongInput = true;
					}
				}
			}

			// 将非法输入代以空字符
			if (isWrongInput)
			{
				charArray[i] = '\0';
			}

			isWrongInput = false;
		}

		hasScale = false;

		inputField.text = new string(charArray);
	}
}
