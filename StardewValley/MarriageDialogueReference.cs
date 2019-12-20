using Netcode;
using System;
using System.Linq;

namespace StardewValley
{
	public class MarriageDialogueReference : INetObject<NetFields>, IEquatable<MarriageDialogueReference>
	{
		public const string ENDEARMENT_TOKEN = "%endearment";

		public const string ENDEARMENT_TOKEN_LOWER = "%endearmentlower";

		private readonly NetString _dialogueFile = new NetString("");

		private readonly NetString _dialogueKey = new NetString("");

		private readonly NetBool _isGendered = new NetBool(value: false);

		private readonly NetStringList _substitutions = new NetStringList();

		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public string DialogueFile => _dialogueFile.Value;

		public string DialogueKey => _dialogueKey.Value;

		public bool IsGendered => _isGendered.Value;

		public string[] Substitutions => _substitutions.ToArray();

		public MarriageDialogueReference()
		{
			NetFields.AddFields(_dialogueFile, _dialogueKey, _isGendered, _substitutions);
		}

		public MarriageDialogueReference(string dialogue_file, string dialogue_key, bool gendered = false, params string[] substitutions)
		{
			_dialogueFile.Value = dialogue_file;
			_dialogueKey.Value = dialogue_key;
			_isGendered.Value = _isGendered;
			if (substitutions.Length != 0)
			{
				_substitutions.AddRange(substitutions);
			}
			NetFields.AddFields(_dialogueFile, _dialogueKey, _isGendered, _substitutions);
		}

		public string GetText()
		{
			return "";
		}

		public bool IsItemGrabDialogue(NPC n)
		{
			return GetDialogue(n).isItemGrabDialogue();
		}

		protected string _ReplaceTokens(string text, NPC n)
		{
			text = text.Replace("%endearment", n.getTermOfSpousalEndearment());
			text = text.Replace("%endearmentlower", n.getTermOfSpousalEndearment().ToLower());
			return text;
		}

		public Dialogue GetDialogue(NPC n)
		{
			if (_dialogueFile.Value.Contains("Marriage"))
			{
				return new Dialogue(_ReplaceTokens(n.tryToGetMarriageSpecificDialogueElseReturnDefault(_dialogueKey.Value), n), n)
				{
					removeOnNextMove = true
				};
			}
			if (_isGendered.Value)
			{
				return new Dialogue(_ReplaceTokens(Game1.LoadStringByGender(n.gender, _dialogueFile.Value + ":" + _dialogueKey.Value, _substitutions), n), n)
				{
					removeOnNextMove = true
				};
			}
			return new Dialogue(_ReplaceTokens(Game1.content.LoadString(_dialogueFile.Value + ":" + _dialogueKey.Value, _substitutions), n), n)
			{
				removeOnNextMove = true
			};
		}

		public bool Equals(MarriageDialogueReference other)
		{
			if (object.Equals(_dialogueFile.Value, other._dialogueFile.Value) && object.Equals(_dialogueKey.Value, other._dialogueKey.Value) && object.Equals(_isGendered.Value, other._isGendered.Value))
			{
				return _substitutions.SequenceEqual(other._substitutions);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is MarriageDialogueReference)
			{
				return Equals(obj as MarriageDialogueReference);
			}
			return false;
		}

		public override int GetHashCode()
		{
			int hash4 = 13;
			hash4 = hash4 * 7 + ((_dialogueFile.Value != null) ? _dialogueFile.Value.GetHashCode() : 0);
			hash4 = hash4 * 7 + ((_dialogueKey.Value != null) ? _dialogueFile.Value.GetHashCode() : 0);
			hash4 = hash4 * 7 + ((!_isGendered.Value) ? 1 : 0);
			foreach (string substitution in _substitutions)
			{
				hash4 = hash4 * 7 + substitution.GetHashCode();
			}
			return hash4;
		}
	}
}
