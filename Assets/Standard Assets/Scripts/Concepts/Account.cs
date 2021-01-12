using System;
using Extensions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ArcherGame
{
	[Serializable]
	public class Account : IDefaultable<Account>, ISaveableAndLoadable
	{
		public int uniqueId;
		public int UniqueId
		{
			get
			{
				return uniqueId;
			}
			set
			{
				uniqueId = value;
			}
		}
		// public string Name
		// {
		// 	get
		// 	{
		// 		return name;
		// 	}
		// 	set
		// 	{
		// 		name = value;
		// 	}
		// }
		public int index;
		// [SaveAndLoadValue(true)]
		// public string name;
		public string Name
		{
			get
			{
				return PlayerPrefs.GetString("Account " + index + " name", "");
			}
			set
			{
				PlayerPrefs.SetString("Account " + index + " name", value);
			}
		}
		// [SaveAndLoadValue(true)]
		// public float playTime;
		public float PlayTime
		{
			get
			{
				return PlayerPrefs.GetFloat("Account " + index + " play time", 0);
			}
			set
			{
				PlayerPrefs.SetFloat("Account " + index + " play time", value);
			}
		}
		// [SaveAndLoadValue(true)]
		// public int totalMoney;
		public int TotalMoney
		{
			get
			{
				return PlayerPrefs.GetInt("Account " + index + " total money", 0);
			}
			set
			{
				PlayerPrefs.SetInt("Account " + index + " total money", value);
			}
		}
		// [SaveAndLoadValue(true)]
		// public int currentMoney;
		public int CurrentMoney
		{
			get
			{
				return PlayerPrefs.GetInt("Account " + index + " current money", 0);
			}
			set
			{
				PlayerPrefs.SetInt("Account " + index + " current money", value);
			}
		}
		// [SaveAndLoadValue(true)]
		// public int obelisksTouched;
		public int ObelisksTouched
		{
			get
			{
				return PlayerPrefs.GetInt("Account " + index + " obelisks touched", 0);
			}
			set
			{
				PlayerPrefs.SetInt("Account " + index + " obelisks touched", value);
			}
		}
		public int MostRecentlyUsedSaveIndex
		{
			get
			{
				return PlayerPrefs.GetInt("Account " + index + " most recently used save index", 0);
			}
			set
			{
				PlayerPrefs.SetInt("Account " + index + " most recently used save index", value);
			}
		}
		public int LastSaveIndex
		{
			get
			{
				return PlayerPrefs.GetInt("Account " + index + " last save index", 0);
			}
			set
			{
				PlayerPrefs.SetInt("Account " + index + " last save index", value);
			}
		}
		// [SaveAndLoadValue(true)]
		// public bool isPlaying;
		[SaveAndLoadValue(false)]
		public string username;
		[SaveAndLoadValue(false)]
		public string password;
		[SaveAndLoadValue(false)]
		public OfflineData offlineData = new OfflineData();
		[SaveAndLoadValue(false)]
		public OnlineData onlineData = new OnlineData();
		
		public Account GetDefault ()
		{
			Account account = this;
			account.Name = "";
			account.PlayTime = 0;
			account.TotalMoney = 0;
			account.CurrentMoney = 0;
			account.ObelisksTouched = 0;
			account.username = "";
			account.password = "";
			account.offlineData = new OfflineData();
			account.onlineData = new OnlineData();
			// account.isPlaying = false;
			return account;
		}

		public virtual void Awake ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (uniqueId == 0)
				{
					do
					{
						uniqueId = Random.Range(int.MinValue, int.MaxValue);
					} while (GameManager.UniqueIds.Contains(uniqueId));
					GameManager.UniqueIds = GameManager.UniqueIds.Add(uniqueId);
				}
				Reset ();
				UpdateData ();
				return;
			}
#endif
		}

		public virtual void Reset ()
		{
			username = "";
			password = "";
			offlineData = new OfflineData();
			onlineData = new OnlineData();
			for (int i = 0; i < ArchivesManager.MAX_ACCOUNTS; i ++)
			{
				offlineData.battleMode.versusHuman.data[i] = new OfflineBattleDataEntry();
				offlineData.battleMode.versusHuman.noSwitchingVariant.data[i] = new OfflineBattleDataEntry();
				offlineData.battleMode.versusHuman.switchPositionsVariant.data[i] = new OfflineBattleDataEntry();
				offlineData.battleMode.versusHuman.switchControlVariant.data[i] = new OfflineBattleDataEntry();
			}
		}

		public virtual void UpdateData ()
		{
			string opponentNameString;
			for (int i = 0; i < ArchivesManager.MAX_ACCOUNTS; i ++)
			{
				offlineData.battleMode.versusHuman.data[i].accountName = username;
				offlineData.battleMode.versusHuman.noSwitchingVariant.data[i].accountName = username;
				offlineData.battleMode.versusHuman.switchPositionsVariant.data[i].accountName = username;
				offlineData.battleMode.versusHuman.switchControlVariant.data[i].accountName = username;
				if (i == ArchivesManager.Instance.localAccountsData.IndexOf(this))
					opponentNameString = "a human";
				else
					opponentNameString = ArchivesManager.Instance.localAccountsData[i].username;
				offlineData.battleMode.versusHuman.data[i].opponentNameString = opponentNameString;
				offlineData.battleMode.versusHuman.noSwitchingVariant.data[i].opponentNameString = opponentNameString;
				offlineData.battleMode.versusHuman.switchPositionsVariant.data[i].opponentNameString = opponentNameString;
				offlineData.battleMode.versusHuman.switchControlVariant.data[i].opponentNameString = opponentNameString;
			}
			offlineData.battleMode.versusAI.data.accountName = username;
			offlineData.battleMode.versusAI.noSwitchingVariant.data.accountName = username;
			offlineData.battleMode.versusAI.switchPositionsVariant.data.accountName = username;
			offlineData.battleMode.versusAI.switchControlVariant.data.accountName = username;
			opponentNameString = "the AI";
			offlineData.battleMode.versusAI.data.opponentNameString = opponentNameString;
			offlineData.battleMode.versusAI.noSwitchingVariant.data.opponentNameString = opponentNameString;
			offlineData.battleMode.versusAI.switchPositionsVariant.data.opponentNameString = opponentNameString;
			offlineData.battleMode.versusAI.switchControlVariant.data.opponentNameString = opponentNameString;
			onlineData.accountName = username;
		}

		public override string ToString ()
		{
			return offlineData.ToString() + onlineData.ToString();
		}

		[Serializable]
		public class OfflineData
		{
			public BattleMode battleMode = new BattleMode();

			public override string ToString ()
			{
				return "-----Offline-----\n" + battleMode.ToString();
			}

			[Serializable]
			public class BattleMode
			{
				public VersusHuman versusHuman = new VersusHuman();
				public VersusAI versusAI = new VersusAI();

				public override string ToString ()
				{
					return versusHuman.ToString() + versusAI.ToString();
				}

				[Serializable]
				public class VersusHuman
				{
					public OfflineBattleDataEntry[] data = new OfflineBattleDataEntry[ArchivesManager.MAX_ACCOUNTS];
					public NoSwitchingVariantVersusHuman noSwitchingVariant = new NoSwitchingVariantVersusHuman();
					public SwitchPositionsVariantVersusHuman switchPositionsVariant = new SwitchPositionsVariantVersusHuman();
					public SwitchControlVariantVersusHuman switchControlVariant = new SwitchControlVariantVersusHuman();

					public override string ToString ()
					{
						string output = "---Versus Human\n";
						foreach (OfflineBattleDataEntry dataPiece in data)
							output += dataPiece.ToString();
						output += noSwitchingVariant.ToString() + switchPositionsVariant.ToString() + switchControlVariant.ToString();
						return output;
					}

					[Serializable]
					public class NoSwitchingVariantVersusHuman
					{
						public OfflineBattleDataEntry[] data = new OfflineBattleDataEntry[ArchivesManager.MAX_ACCOUNTS];

						public override string ToString ()
						{
							string output = "- No Switching\n";
							foreach (OfflineBattleDataEntry dataPiece in data)
								output += dataPiece.ToString();
							return output;
						}
					}

					[Serializable]
					public class SwitchPositionsVariantVersusHuman
					{
						public OfflineBattleDataEntry[] data = new OfflineBattleDataEntry[ArchivesManager.MAX_ACCOUNTS];

						public override string ToString ()
						{
							string output = "- Switch Positions\n";
							foreach (OfflineBattleDataEntry dataPiece in data)
								output += dataPiece.ToString();
							return output;
						}
					}

					[Serializable]
					public class SwitchControlVariantVersusHuman
					{
						public OfflineBattleDataEntry[] data = new OfflineBattleDataEntry[ArchivesManager.MAX_ACCOUNTS];

						public override string ToString ()
						{
							string output = "- Switch Control\n";
							foreach (OfflineBattleDataEntry dataPiece in data)
								output += dataPiece.ToString();
							return output;
						}
					}
				}

				[Serializable]
				public class VersusAI
				{
					public OfflineBattleDataEntry data = new OfflineBattleDataEntry();
					public NoSwitchingVariantVersusAI noSwitchingVariant = new NoSwitchingVariantVersusAI();
					public SwitchPositionsVariantVersusAI switchPositionsVariant = new SwitchPositionsVariantVersusAI();
					public SwitchControlVariantVersusAI switchControlVariant = new SwitchControlVariantVersusAI();

					public override string ToString ()
					{
						return "---Versus AI\n" + data.ToString() + noSwitchingVariant.ToString() + switchPositionsVariant.ToString() + switchControlVariant.ToString();
					}

					[Serializable]
					public class NoSwitchingVariantVersusAI
					{
						public OfflineBattleDataEntry data = new OfflineBattleDataEntry();

						public override string ToString ()
						{
							return "- No Switching\n" + data.ToString();
						}
					}

					[Serializable]
					public class SwitchPositionsVariantVersusAI
					{
						public OfflineBattleDataEntry data = new OfflineBattleDataEntry();

						public override string ToString ()
						{
							return "- Switch Positions\n" + data.ToString();
						}
					}

					[Serializable]
					public class SwitchControlVariantVersusAI
					{
						public OfflineBattleDataEntry data = new OfflineBattleDataEntry();

						public override string ToString ()
						{
							return "- Switch Control\n" + data.ToString();
						}
					}
				}
			}
		}

		[Serializable]
		public class OnlineData
		{
			public string accountName;
			public uint kills;
			public uint deaths;
			public float killDeathRatio;
			public uint highscore;

			public override string ToString ()
			{
				string output = "-----Online-----\n";
				output += accountName + " has " + kills + " kills and " + deaths + " deaths.";
				output += " This means that " + accountName + " has a kill-death-ratio of " + killDeathRatio + ".";
				output += " The highscore of " + accountName + " is " + highscore + ".";
				return output;
			}
		}

		[Serializable]
		public class OfflineBattleDataEntry
		{
			public string opponentNameString;
			public string accountName;
			public uint totalFairKills;
			public uint totalFairDeaths;
			public float fairKillDeathRatio;
			public uint maxSpeedDisadvantage;

			public override string ToString ()
			{
				string output = "";
				if (!string.IsNullOrEmpty(opponentNameString))
				{
					output += accountName + " has killed " + opponentNameString + " " + totalFairKills + " times fairly (their move speeds were equal)";
					output += " and " + accountName + " has been killed by " + opponentNameString + " " + totalFairDeaths + " times fairly.";
					output += " This means that in fair fights against " + opponentNameString + ", " + accountName + " has a kill-death-ratio of " + fairKillDeathRatio + ".";
					output += " The highest number of speed disadvantages applied simultaneously to " + accountName + " in unfair fights against " + opponentNameString + " is " + maxSpeedDisadvantage + ".\n";
				}
				return output;
			}
		}
	}
}