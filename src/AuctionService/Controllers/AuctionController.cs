using AuctionService.Data;
using AuctionService.Dtos;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
    [ApiController]
    [Route("api/auctions")]
    public class AuctionController : ControllerBase
    {
        private readonly AuctionDbContext _context;
        private readonly IMapper _mapper;

        public AuctionController(AuctionDbContext context, IMapper mapper)
        {
            this._context = context;
            this._mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<AuctionDto>>> getAllAuctions()
        {
            var auctions = await _context.Auctions.Include(x => x.Item).OrderBy(x => x.Item.Make).ToListAsync();

            var auctionsToSend = _mapper.Map<List<AuctionDto>>(auctions);

            return Ok(auctionsToSend);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> getAuctionById(Guid id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            var auction = await _context.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);

            if (auction == null)
            {
                return NotFound();
            }

            var auctionToSend = _mapper.Map<AuctionDto>(auction);

            return Ok(auctionToSend);
        }

        [HttpPost]
        public async Task<ActionResult<AuctionDto>> createAuction(CreateAuctionDto createAuction)
        {
            var auction = _mapper.Map<Auction>(createAuction);

            //TODO: Add current user as Seller
            auction.Seller = "Seller";

            _context.Auctions.Add(auction);

            var result = await _context.SaveChangesAsync() > 0;

            if (result)
            {
                return CreatedAtAction(nameof(getAuctionById), new { auction.Id }, _mapper.Map<AuctionDto>(auction));
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> updateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
        {
            var auction = await _context.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);

            if (auction == null)
            {
                return NotFound();
            }

            auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
            auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
            auction.Item.Year = updateAuctionDto.Year != 0 ? updateAuctionDto.Year : auction.Item.Year;
            auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
            auction.Item.Mileage = updateAuctionDto.Mileage != 0 ? updateAuctionDto.Mileage : auction.Item.Mileage;

            var result = await _context.SaveChangesAsync() > 0;

            if (result)
            {
                return NoContent();
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAuction(Guid id)
        {
            var auction = await _context.Auctions.FirstOrDefaultAsync(x => x.Id == id);

            if (auction == null)
            {
                return NotFound();
            }

            _context.Auctions.Remove(auction);

            var result = await _context.SaveChangesAsync() > 0;

            if (result)
            {
                return NoContent();
            }
            else
            {
                return BadRequest();
            }
        }

    }
}
