using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Contracts;
using Newtonsoft.Json;
using UnityEngine;
using PackContract = Thirdweb.Contracts.Pack.ContractDefinition;

namespace Thirdweb
{
    /// <summary>
    /// Interact with a Pack contract.
    /// </summary>
    public class Pack : Routable
    {
        private readonly string _contractAddress;
        private readonly ThirdwebSDK _sdk;

        /// <summary>
        /// Interact with a Marketplace contract.
        /// </summary>
        public Pack(ThirdwebSDK sdk, string address)
            : base($"{address}{subSeparator}pack")
        {
            this._contractAddress = address;
            this._sdk = sdk;
        }

        /// READ FUNCTIONS

        /// <summary>
        /// Get a NFT in this contract by its ID
        /// </summary>
        public async Task<NFT> Get(string tokenId)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<NFT>(getRoute("get"), Utils.ToJsonStringArray(tokenId));
            }
            else
            {
                var tokenURI = await TransactionManager.ThirdwebRead<PackContract.UriFunction, PackContract.UriOutputDTO>(
                    _sdk,
                    _contractAddress,
                    new PackContract.UriFunction() { TokenId = BigInteger.Parse(tokenId) }
                );

                var nft = new NFT
                {
                    owner = "",
                    type = "ERC1155",
                    supply = await TotalSupply(tokenId),
                    quantityOwned = null,
                    metadata = await _sdk.Storage.DownloadText<NFTMetadata>(tokenURI.ReturnValue1)
                };
                nft.metadata.image = nft.metadata.image.ReplaceIPFS(_sdk.Storage.IPFSGateway);
                nft.metadata.id = tokenId;
                nft.metadata.uri = tokenURI.ReturnValue1.ReplaceIPFS(_sdk.Storage.IPFSGateway);
                return nft;
            }
        }

        /// <summary>
        /// Get a all NFTs in this contract
        /// </summary>
        public async Task<List<NFT>> GetAll(QueryAllParams queryParams = null)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<List<NFT>>(getRoute("getAll"), Utils.ToJsonStringArray(queryParams));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }

        /// <summary>
        /// Get a all NFTs owned by the connected wallet
        /// </summary>
        /// <param name="address">Optional wallet address to query NFTs of</param>
        public async Task<List<NFT>> GetOwned(string address = null)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<List<NFT>>(getRoute("getOwned"), Utils.ToJsonStringArray(address));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }

        /// <summary>
        /// Get the balance of the given NFT for the connected wallet
        /// </summary>
        public async Task<BigInteger> Balance(string tokenId)
        {
            return await BalanceOf(await _sdk.Wallet.GetAddress(), tokenId);
        }

        /// <summary>
        /// Get the balance of the given NFT for the given wallet address
        /// </summary>
        public async Task<BigInteger> BalanceOf(string address, string tokenId)
        {
            if (Utils.IsWebGLBuild())
            {
                var val = await Bridge.InvokeRoute<string>(getRoute("balanceOf"), Utils.ToJsonStringArray(address, tokenId));
                return BigInteger.Parse(val);
            }
            else
            {
                var tokenURI = await TransactionManager.ThirdwebRead<PackContract.BalanceOfFunction, PackContract.BalanceOfOutputDTO>(
                    _sdk,
                    _contractAddress,
                    new PackContract.BalanceOfFunction() { Account = address, Id = BigInteger.Parse(tokenId) }
                );
                return tokenURI.ReturnValue1;
            }
        }

        /// <summary>
        /// Check whether the given contract address has been approved to transfer NFTs on behalf of the given wallet address
        /// </summary>
        /// <param name="address">The wallet address</param>
        /// <param name="contractAddress">The contract address to check approval for</param>
        public async Task<string> IsApprovedForAll(string address, string approvedContract)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<string>(getRoute("isApproved"), Utils.ToJsonStringArray(address, approvedContract));
            }
            else
            {
                var isApprovedForAll = await TransactionManager.ThirdwebRead<PackContract.IsApprovedForAllFunction, PackContract.IsApprovedForAllOutputDTO>(
                    _sdk,
                    _contractAddress,
                    new PackContract.IsApprovedForAllFunction() { Account = address, Operator = approvedContract }
                );
                return isApprovedForAll.ReturnValue1.ToString();
            }
        }

        public async Task<BigInteger> TotalCount()
        {
            if (Utils.IsWebGLBuild())
            {
                var val = await Bridge.InvokeRoute<string>(getRoute("totalCount"), new string[] { });
                return BigInteger.Parse(val);
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }

        /// <summary>
        /// Get the total suppply in circulation for thge given NFT
        /// </summary>
        public async Task<BigInteger> TotalSupply(string tokenId)
        {
            if (Utils.IsWebGLBuild())
            {
                var val = await Bridge.InvokeRoute<string>(getRoute("totalSupply"), Utils.ToJsonStringArray(tokenId));
                return BigInteger.Parse(val);
            }
            else
            {
                var totalSupply = await TransactionManager.ThirdwebRead<PackContract.TotalSupplyFunction, PackContract.TotalSupplyOutputDTO>(
                    _sdk,
                    _contractAddress,
                    new PackContract.TotalSupplyFunction() { ReturnValue1 = BigInteger.Parse(tokenId) }
                );
                return totalSupply.ReturnValue1;
            }
        }

        /// <summary>
        /// Get all the possible contents of a given pack
        /// </summary>
        public async Task<PackContents> GetPackContents(string packId)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<PackContents>(getRoute("getPackContents"), Utils.ToJsonStringArray(packId));
            }
            else
            {
                var packContents = await TransactionManager.ThirdwebRead<PackContract.GetPackContentsFunction, PackContract.GetPackContentsOutputDTO>(
                    _sdk,
                    _contractAddress,
                    new PackContract.GetPackContentsFunction() { PackId = BigInteger.Parse(packId) }
                );
                var erc20R = new List<ERC20Contents>();
                var erc721R = new List<ERC721Contents>();
                var erc1155R = new List<ERC1155Contents>();
                for (int i = 0; i < packContents.Contents.Count; i++)
                {
                    var tokenReward = packContents.Contents[i];
                    var amount = packContents.PerUnitAmounts[i];
                    switch (tokenReward.TokenType)
                    {
                        case 0:
                            var tempERC20 = new ERC20Contents
                            {
                                contractAddress = tokenReward.AssetContract,
                                quantityPerReward = amount.ToString().ToEth(18),
                                totalRewards = (tokenReward.TotalAmount / amount).ToString().ToEth(18)
                            };
                            erc20R.Add(tempERC20);
                            break;
                        case 1:
                            var tempERC721 = new ERC721Contents { contractAddress = tokenReward.AssetContract, tokenId = tokenReward.TokenId.ToString() };
                            erc721R.Add(tempERC721);
                            break;
                        case 2:
                            var tempERC1155 = new ERC1155Contents
                            {
                                contractAddress = tokenReward.AssetContract,
                                tokenId = tokenReward.TokenId.ToString(),
                                quantityPerReward = amount.ToString(),
                                totalRewards = (tokenReward.TotalAmount / amount).ToString()
                            };
                            erc1155R.Add(tempERC1155);
                            break;
                        default:
                            break;
                    }
                }
                var contents = new PackContents
                {
                    erc20Rewards = erc20R,
                    erc721Rewards = erc721R,
                    erc1155Rewards = erc1155R
                };
                return contents;
            }
        }

        /// WRITE FUNCTIONS

        /// <summary>
        /// Set approval to the given contract to transfer NFTs on behalf of the connected wallet
        /// </summary>
        public async Task<TransactionResult> SetApprovalForAll(string contractToApprove, bool approved)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("isApproved"), Utils.ToJsonStringArray(contractToApprove, approved));
            }
            else
            {
                return await TransactionManager.ThirdwebWrite(_sdk, _contractAddress, new PackContract.SetApprovalForAllFunction() { Operator = contractToApprove, Approved = approved });
            }
        }

        /// <summary>
        /// Transfer NFTs to the given address
        /// </summary>
        public async Task<TransactionResult> Transfer(string to, string tokenId, int amount)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("transfer"), Utils.ToJsonStringArray(to, tokenId, amount));
            }
            else
            {
                return await TransactionManager.ThirdwebWrite(
                    _sdk,
                    _contractAddress,
                    new PackContract.SafeTransferFromFunction()
                    {
                        From = await _sdk.Wallet.GetAddress(),
                        To = to,
                        Id = BigInteger.Parse(tokenId),
                        Amount = amount,
                        Data = new byte[0]
                    }
                );
            }
        }

        /// <summary>
        /// Create a new Pack with all the possible rewards (requires approval to transfer tokens/NFTs defined as rewards)
        /// </summary>
        public async Task<TransactionResult> Create(NewPackInput pack)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("create"), Utils.ToJsonStringArray(pack));
            }
            else
            {
                return await CreateTo(await _sdk.Wallet.GetAddress(), pack);
            }
        }

        /// <summary>
        /// Create a new Pack with all the possible rewards and mints it to the given address (requires approval to transfer tokens/NFTs defined as rewards)
        /// </summary>
        public async Task<TransactionResult> CreateTo(string receiverAddress, NewPackInput pack)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("createTo"), Utils.ToJsonStringArray(receiverAddress, pack));
            }
            else
            {
                var uri = await _sdk.Storage.UploadText(JsonConvert.SerializeObject(pack.packMetadata));
                return await TransactionManager.ThirdwebWrite(
                    _sdk,
                    _contractAddress,
                    new PackContract.CreatePackFunction()
                    {
                        Contents = pack.ToPackTokenList(),
                        NumOfRewardUnits = pack.ToPackRewardUnitsList(),
                        PackUri = uri.IpfsHash.CidToIpfsUrl(),
                        OpenStartTimestamp = await _sdk.Blocks.GetLatestBlockTimestamp(),
                        AmountDistributedPerOpen = BigInteger.Parse(pack.rewardsPerPack),
                        Recipient = receiverAddress
                    }
                );
            }
        }

        /// <summary>
        /// Add new contents to an existing pack
        /// </summary>
        public async Task<TransactionResult> AddPackContents(string packId, PackRewards newContents)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("addPackContents"), Utils.ToJsonStringArray(packId, newContents));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }

        /// <summary>
        /// Open a pack and transfer the rewards to the connected wallet
        /// </summary>
        public async Task<PackRewards> Open(string packId, string amount = "1", int gasLimit = 1000000)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<PackRewards>(getRoute("open"), Utils.ToJsonStringArray(packId, amount));
            }
            else
            {
                var openPackFunction = new PackContract.OpenPackFunction() { PackId = BigInteger.Parse(packId), AmountToOpen = BigInteger.Parse(amount) };
                var openPackResult = await TransactionManager.ThirdwebWriteRawResult(_sdk, _contractAddress, openPackFunction, null, gasLimit);

                var packOpenedEvents = openPackResult.DecodeAllEvents<PackContract.PackOpenedEventDTO>();
                var tokensAwarded = new List<PackContract.Token>();
                foreach (var packOpenedEvent in packOpenedEvents)
                {
                    tokensAwarded.AddRange(packOpenedEvent.Event.RewardUnitsDistributed);
                }
                var packRewards = new PackRewards()
                {
                    erc20Rewards = new List<ERC20Reward>(),
                    erc721Rewards = new List<ERC721Reward>(),
                    erc1155Rewards = new List<ERC1155Reward>()
                };
                foreach (var tokenAwarded in tokensAwarded)
                {
                    if (tokenAwarded.TokenType == 0)
                    {
                        packRewards.erc20Rewards.Add(new ERC20Reward() { contractAddress = tokenAwarded.AssetContract, quantityPerReward = tokenAwarded.TotalAmount.ToString() });
                    }
                    else if (tokenAwarded.TokenType == 1)
                    {
                        packRewards.erc721Rewards.Add(new ERC721Reward() { contractAddress = tokenAwarded.AssetContract, tokenId = tokenAwarded.TokenId.ToString() });
                    }
                    else if (tokenAwarded.TokenType == 2)
                    {
                        packRewards.erc1155Rewards.Add(
                            new ERC1155Reward()
                            {
                                contractAddress = tokenAwarded.AssetContract,
                                tokenId = tokenAwarded.TokenId.ToString(),
                                quantityPerReward = tokenAwarded.TotalAmount.ToString()
                            }
                        );
                    }
                }
                return packRewards;
            }
        }
    }

    [System.Serializable]
    public class PackRewards
    {
        public List<ERC20Reward> erc20Rewards;
        public List<ERC721Reward> erc721Rewards;
        public List<ERC1155Reward> erc1155Rewards;

        public override string ToString()
        {
            string erc20str = "ERC20 Rewards:\n";
            foreach (var reward in erc20Rewards)
                erc20str += reward.ToString();
            string erc721str = "ERC721 Rewards:\n";
            foreach (var reward in erc721Rewards)
                erc721str += reward.ToString();
            string erc1155str = "ERC1155 Rewards:\n";
            foreach (var reward in erc1155Rewards)
                erc1155str += reward.ToString();
            return "PackRewards:\n" + erc20str + erc721str + erc1155str;
        }
    }

    [System.Serializable]
    public class PackContents
    {
        public List<ERC20Contents> erc20Rewards;
        public List<ERC721Contents> erc721Rewards;
        public List<ERC1155Contents> erc1155Rewards;

        public override string ToString()
        {
            string erc20str = "\n";
            foreach (var content in erc20Rewards)
                erc20str += content.ToString();
            string erc721str = "\n";
            foreach (var content in erc721Rewards)
                erc721str += content.ToString();
            string erc1155str = "\n";
            foreach (var content in erc1155Rewards)
                erc1155str += content.ToString();
            return "PackContents:\n" + erc20str + erc721str + erc1155str;
        }
    }

    [System.Serializable]
    public class NewPackInput : PackContents
    {
        /// The Metadata of the pack NFT itself
        public NFTMetadata packMetadata;

        /// How many rewards can be obtained by opening a single pack
        public string rewardsPerPack;

        public override string ToString()
        {
            return "NewPackInput:\n" + $"packMetadata: {packMetadata}\n" + $"rewardsPerPack: {rewardsPerPack}\n";
        }
    }

    [System.Serializable]
    public class ERC20Reward
    {
        /// the Token contract address
        public string contractAddress;

        /// How many tokens can be otained when opening a pack and receiving this reward
        public string quantityPerReward;

        public override string ToString()
        {
            return "ERC20Reward:\n" + $"contractAddress: {contractAddress}\n" + $"quantityPerReward: {quantityPerReward}\n";
        }
    }

    [System.Serializable]
    public class ERC20Contents : ERC20Reward
    {
        public string totalRewards;

        public override string ToString()
        {
            return base.ToString() + $"totalRewards: {totalRewards}\n";
        }
    }

    [System.Serializable]
    public class ERC721Reward
    {
        /// the ERC721 contract address
        public string contractAddress;

        /// the tokenId of the NFT to be rewarded
        public string tokenId;

        public override string ToString()
        {
            return "ERC721Reward:\n" + $"contractAddress: {contractAddress}\n" + $"tokenId: {tokenId}\n";
        }
    }

    [System.Serializable]
    public class ERC721Contents : ERC721Reward
    {
        public override string ToString()
        {
            return base.ToString();
        }
    }

    [System.Serializable]
    public class ERC1155Reward
    {
        /// the ERC1155 contract address
        public string contractAddress;

        /// the tokenId of the NFT to be rewarded
        public string tokenId;

        /// How many NFTs can be otained when opening a pack and receiving this reward
        public string quantityPerReward;

        public override string ToString()
        {
            return "ERC1155Reward:\n" + $"contractAddress: {contractAddress}\n" + $"tokenId: {tokenId}\n" + $"quantityPerReward: {quantityPerReward}\n";
        }
    }

    [System.Serializable]
    public class ERC1155Contents : ERC1155Reward
    {
        public string totalRewards;

        public override string ToString()
        {
            return base.ToString() + $"totalRewards: {totalRewards}\n";
        }
    }
}
