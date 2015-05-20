#include "stdafx.h";
#include <iostream>
#include <algorithm>
#include <vector>
using namespace std;
	bool myfunction(int i, int j) { return (i<j); }
class Solution {
public:
	vector<int> twoSum(vector<int> &numbers, int target) {
		vector<int> ret(2);
		vector<int> sortVec = numbers;
		sort(sortVec.begin(), sortVec.end(),myfunction);
		int length = numbers.size();
		for (int index = 0; index<sortVec.size() - 1; index++)
		{
			int retIndex = GetNumberIndex(sortVec, index + 1, length - 1, target - sortVec[index]);
			if (retIndex>0)
			{
				ret[0] = sortVec[index];
				ret[1] = sortVec[retIndex];
				break;
			}
		}
		ret[0] = GetIndexOfNumbers(numbers, ret[0],-1);
		ret[1] = GetIndexOfNumbers(numbers, ret[1],ret[0]);
		if (ret[0] > ret[1])
		{ 
			int t = ret[0];
			ret[0] = ret[1];
			ret[1] = t;
		}
		return ret;
	}
	int GetIndexOfNumbers(vector<int>numbers, int value,int isUsed)
	{
		for (int index = 0; index < numbers.size(); ++index)
		{
			if (numbers[index] == value && index!=isUsed-1)
				return index+1;
		}
		return -1;
	}
	int GetNumberIndex(vector<int> &numbers, int begin, int end, int target)
	{
		if (begin >= end)
		{
			if (numbers[begin] == target)
				return begin;
			else
				return -1;
		}
		int mid = (begin + end) / 2;
		if (numbers[mid] == target)
			return mid;
		else if (numbers[mid] > target)
		{
			return GetNumberIndex(numbers, begin, mid - 1, target);
		}
		else
		{
			return GetNumberIndex(numbers, mid + 1, end, target);
		}

	}
};
void main()
{
	vector<int> test(4);
	test[0] = 0;
	test[1] = 2;
	test[2] = 5;
	test[3] = 0;
	Solution solution =  Solution();
    vector<int>ret =solution.twoSum(test,0);
	cout << ret[0] << " " << ret[1] << endl;
}
