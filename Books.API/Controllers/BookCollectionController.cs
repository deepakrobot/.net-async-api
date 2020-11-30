using AutoMapper;
using Books.API.Filters;
using Books.API.ModelBinders;
using Books.API.Models;
using Books.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Books.API.Controllers
{
    [ApiController]
    [Route("api/bookscollection")]
    [BooksResultFilter]
    public class BookCollectionController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;
        private readonly IMapper _mapper;

        public BookCollectionController(IBookRepository bookRepository, IMapper mapper)
        {
            _bookRepository = bookRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("{bookids}", Name ="GetBookCollection")]
        public async Task<IActionResult> GetBookCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))]IEnumerable<Guid> bookIds)
        {
            var bookEntities = await _bookRepository.GetBooksAsync(bookIds);
            if (bookEntities.Count() != bookIds.Count())
            {
                return NotFound();
            }

            return Ok(bookEntities);

        }

        [HttpPost]
        public async Task<IActionResult> CreateBookCollection(IEnumerable<BookForCreation> bookCollection)
        {
            var bookEntities = _mapper.Map<IEnumerable<Entities.Book>>(bookCollection);
            foreach (var book in bookEntities)
            {
                _bookRepository.AddBook(book);
            }
            await _bookRepository.SaveChangesAsync();

            var booksToReturn = await _bookRepository.GetBooksAsync(bookEntities.Select(b => b.Id).ToList());

            var bookIds = String.Join(",", booksToReturn.Select(b => b.Id));
            return CreatedAtRoute("GetBookCollection", new { bookIds}, booksToReturn);
        }
    }
}
