// Dynamic background gradients based on book cover colors
let currentBookIndex = 0;
let top10Books = [];
let bookGradients = []; // To store calculated gradients
let colorThief = new ColorThief(); // For extracting colors from images
let preloadedImages = []; // To cache images for smooth transitions

document.addEventListener('DOMContentLoaded', async function () {
    // 1. Populate top10Books from the data embedded in the CSHTML
    if (!top10Books.length && window.__top10BooksJson) {
        try {
            top10Books = window.__top10BooksJson;
        } catch (e) {
            console.error("JS: Error processing __top10BooksJson:", e);
        }
    }

    // 2. Initialize all components in the correct order
    await initializeHero(); // This now waits for color extraction
    initializeStarDisplays(); // Initialize all star ratings
    initializeHorizontalSliders(); // Initialize the horizontal sliders and their arrows
});

async function initializeHero() {
    console.log("JS: initializeHero called. Current top10Books.length:", top10Books.length);
    if (top10Books && top10Books.length > 0) {
        await extractColorsFromBooks();
        enableNavigation();
    }
}

async function extractColorsFromBooks() {
    if (top10Books.length === 0) return;
    bookGradients = [];
    preloadedImages = [];
    const imageLoadPromises = top10Books.map((book, index) => new Promise((resolve) => {
        const img = new Image();
        img.crossOrigin = 'anonymous';
        img.onload = function () {
            try {
                preloadedImages[index] = img;
                const palette = colorThief.getPalette(img, 3);
                const gradient = `linear-gradient(135deg, rgb(${palette[0][0]}, ${palette[0][1]}, ${palette[0][2]}) 0%, rgb(${palette[1][0]}, ${palette[1][1]}, ${palette[1][2]}) 50%, rgb(${palette[2][0]}, ${palette[2][1]}, ${palette[2][2]}) 100%)`;
                bookGradients[index] = gradient;
                resolve({ index, gradient });
            } catch (error) {
                const fallbackGradient = 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)';
                bookGradients[index] = fallbackGradient;
                preloadedImages[index] = img;
                resolve({ index, gradient: fallbackGradient });
            }
        };
        img.onerror = function () {
            const fallbackGradient = 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)';
            bookGradients[index] = fallbackGradient;
            resolve({ index, gradient: fallbackGradient });
        };
        img.src = book.coverImage;
    }));
    await Promise.all(imageLoadPromises);
    if (bookGradients[0]) document.querySelector('.netflix-container').style.background = bookGradients[0];
}

function initializeStarDisplays() {
    document.querySelectorAll('.star-rating-display').forEach(updateStarDisplay);
}

function updateStarDisplay(container) {
    const rating = parseFloat(container.getAttribute('data-rating')) || 0;
    container.querySelectorAll('.star').forEach(function (star) {
        const v = parseInt(star.getAttribute('data-value'));
        if (v <= rating) star.classList.add('filled');
        else star.classList.remove('filled');
    });
}

function enableNavigation() {
    const l = document.getElementById('heroNavLeft');
    const r = document.getElementById('heroNavRight');
    if (l && r) {
        l.disabled = top10Books.length <= 1;
        r.disabled = top10Books.length <= 1;
    }
}

function changeHeroBook(direction) {
    if (top10Books.length === 0) return;
    const leftButton = document.getElementById('heroNavLeft');
    if (leftButton && leftButton.disabled && top10Books.length > 1) {
        leftButton.disabled = false;
        document.getElementById('heroNavRight').disabled = false;
    }

    const heroBook = document.getElementById('heroBook');
    heroBook.classList.add('fade-out');

    setTimeout(() => {
        currentBookIndex = (currentBookIndex + direction + top10Books.length) % top10Books.length;
        const gradient = bookGradients[currentBookIndex];
        if (gradient) document.querySelector('.netflix-container').style.background = gradient;

        const book = top10Books[currentBookIndex];
        const bookCover = document.getElementById('heroBookCover');
        const bookDescription = document.getElementById('heroBookDescription');

        if (preloadedImages[currentBookIndex]) {
            if (bookCover.tagName === 'IMG') {
                bookCover.src = preloadedImages[currentBookIndex].src;
            } else {
                const newImg = document.createElement('img');
                newImg.src = preloadedImages[currentBookIndex].src;
                newImg.alt = book.title;
                newImg.className = 'hero-book-cover';
                newImg.id = 'heroBookCover';
                bookCover.parentNode.replaceChild(newImg, bookCover);
            }
        } else if (book.coverImage) {
            if (bookCover.tagName === 'IMG') {
                bookCover.src = book.coverImage;
            } else {
                const newImg = document.createElement('img');
                newImg.src = book.coverImage;
                newImg.alt = book.title;
                newImg.className = 'hero-book-cover';
                newImg.id = 'heroBookCover';
                bookCover.parentNode.replaceChild(newImg, bookCover);
            }
        } else {
            if (bookCover.tagName === 'IMG') {
                const placeholder = document.createElement('div');
                placeholder.className = 'hero-book-cover book-cover-placeholder';
                placeholder.id = 'heroBookCover';
                placeholder.innerHTML = '<i class="fas fa-book"></i>';
                bookCover.parentNode.replaceChild(placeholder, bookCover);
            }
        }

        document.getElementById('heroBookTitle').textContent = book.title;
        document.getElementById('heroBookAuthor').textContent = 'by ' + book.author;

        if (book.description && book.description.trim() !== '') {
            if (bookDescription) {
                bookDescription.textContent = book.description;
                bookDescription.style.display = 'block';
            } else {
                const desc = document.createElement('p');
                desc.className = 'book-description';
                desc.id = 'heroBookDescription';
                desc.textContent = book.description;
                document.getElementById('heroBookAuthor').parentNode.insertBefore(desc, document.getElementById('heroBookAuthor').nextSibling);
            }
        } else if (bookDescription) {
            bookDescription.style.display = 'none';
        }

        const heroRatingContainer = document.getElementById('heroBookRating');
        heroRatingContainer.setAttribute('data-rating', book.averageRating || 0);
        updateStarDisplay(heroRatingContainer);
        document.getElementById('heroBookBorrows').textContent = book.borrowCount + ' borrows';

        heroBook.classList.remove('fade-out');
        heroBook.classList.add('fade-in');
        setTimeout(() => {
            heroBook.classList.remove('fade-in');
        }, 500);
    }, 250);
}

function scrollRow(button, direction) {
    const container = button.parentElement;
    const slider = container.querySelector('.books-slider');
    const scrollAmount = 220;
    slider.scrollBy({
        left: direction * scrollAmount,
        behavior: 'smooth'
    });
    updateArrowStates(container);
}

function updateArrowStates(container) {
    const slider = container.querySelector('.books-slider');
    const leftArrow = container.querySelector('.scroll-arrow.left');
    const rightArrow = container.querySelector('.scroll-arrow.right');
    leftArrow.disabled = slider.scrollLeft <= 0;
    rightArrow.disabled = slider.scrollLeft >= (slider.scrollWidth - slider.clientWidth);
}

function initializeHorizontalSliders() {
    document.querySelectorAll('.netflix-row').forEach(row => {
        updateArrowStates(row.querySelector('.row-container'));
    });

    let startX = 0;
    let scrollLeft = 0;
    document.querySelectorAll('.books-slider').forEach(slider => {
        slider.addEventListener('touchstart', function (e) {
            startX = e.touches[0].pageX - slider.offsetLeft;
            scrollLeft = slider.scrollLeft;
        });
        slider.addEventListener('touchmove', function (e) {
            e.preventDefault();
            const x = e.touches[0].pageX - slider.offsetLeft;
            const walk = (x - startX) * 2;
            slider.scrollLeft = scrollLeft - walk;
        });
    });
}
