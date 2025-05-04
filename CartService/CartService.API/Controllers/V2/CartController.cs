using CartService.Application.Interfaces;
using CartService.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace CartService.API.Controllers.V2
{
    [ApiController]
    [Route("api/v2/carts")]
    [ApiExplorerSettings(GroupName = "v2")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        /// <summary>
        /// Gets the list of items in the cart (no cart metadata).
        /// </summary>
        /// <param name="cartId">Cart identifier</param>
        /// <returns>List of cart items</returns>
        [HttpGet("{cartId}/items")]
        public async Task<ActionResult<List<CartItem>>> GetItems(string cartId)
        {
            var items = await _cartService.GetCartItemsAsync(cartId);
            return Ok(items);
        }

        /// <summary>
        /// Adds an item to the cart. Creates cart if it doesn't exist.
        /// </summary>
        /// <param name="cartId">Cart identifier</param>
        /// <param name="item">Item to add</param>
        [HttpPost("{cartId}/items")]
        public async Task<IActionResult> AddItem(string cartId, [FromBody] CartItem item)
        {
            await _cartService.AddItemAsync(cartId, item);
            return Ok();
        }

        /// <summary>
        /// Removes an item from the cart by decreasing quantity or removing it.
        /// </summary>
        /// <param name="cartId">Cart identifier</param>
        /// <param name="itemId">ID of the item to remove</param>
        [HttpDelete("{cartId}/items/{itemId:int}")]
        public async Task<IActionResult> RemoveItem(string cartId, int itemId)
        {
            try
            {
                await _cartService.RemoveItemAsync(cartId, itemId);
                return Ok();
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }
    }
}
