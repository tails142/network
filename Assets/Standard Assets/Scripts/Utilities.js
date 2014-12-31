#pragma strict

static function RandomDigits(a_count : int)
{

	var l_result : String;
	var l_index : int = 0;
	
	for (l_index = 0; l_index < a_count; l_index++)
	{
		var l_digit : int = Random.Range(0,9);
		l_result = l_result + l_digit;
	}

	return l_result;
}