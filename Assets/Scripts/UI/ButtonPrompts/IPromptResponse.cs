using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPromptRespone
{
	bool CheckResponse();
	void Enter();
	void Execute();
	void Exit();
	string InteractString();
}