(function ($) {
    'use strict';

    const page = $('.book-details-page');
    if (!page.length) {
        return;
    }

    const config = {
        addToCartUrl: page.data('add-to-cart-url'),
        cartCountUrl: page.data('cart-count-url'),
        rateBookUrl: page.data('rate-book-url'),
        likeReviewUrl: page.data('like-review-url'),
        wishlistStatusUrl: page.data('wishlist-status-url'),
        toggleWishlistUrl: page.data('toggle-wishlist-url')
    };

    const interactiveRating = $('.star-rating-interactive');
    const reviewInput = $('#reviewTextInput');

    $(function () {
        extractAndApplyGradient();
        updateStarDisplay(interactiveRating);
        updateStarDisplay('.star-rating-display');

        const borrowButton = $('#borrowBookButton');
        if (borrowButton.length && config.addToCartUrl) {
            borrowButton.on('click', function (event) {
                event.preventDefault();

                if (borrowButton.hasClass('loading')) {
                    return;
                }

                const bookId = borrowButton.data('book-id');
                borrowButton.addClass('loading').prop('disabled', true);

                $.ajax({
                    url: config.addToCartUrl,
                    type: 'POST',
                    data: { bookId: bookId },
                    success: function (response) {
                        if (response && response.success) {
                            showMessage(response.message, 'success');

                            const originalText = borrowButton.text();
                            borrowButton.text('ADDED!').addClass('added');
                            updateCartBadge();

                            setTimeout(function () {
                                borrowButton.text(originalText).removeClass('added');
                            }, 2000);
                        } else if (response && response.message) {
                            showMessage(response.message, 'error');
                        } else {
                            showMessage('Unable to add book to cart right now.', 'error');
                        }
                    },
                    error: function () {
                        showMessage('Unable to add book to cart right now.', 'error');
                    },
                    complete: function () {
                        borrowButton.removeClass('loading').prop('disabled', false);
                    }
                });
            });
        }

        updateCartBadge();
        initializeWishlistButton();

        reviewInput.on('input', function () {
            $('#charCount').text($(this).val().length);
        });

        $('.star-rating-interactive .star').on('click', function () {
            const star = $(this);
            const container = star.parent();
            const selectedValue = Number(star.data('value'));
            const currentValue = Number(container.data('user-rating')) || 0;
            const nextValue = selectedValue === currentValue ? 0 : selectedValue;

            container.data('user-rating', nextValue);
            updateStarDisplay(container);
        });

        $('.star-rating-interactive .star').on('mouseenter', function () {
            const hoverValue = Number($(this).data('value'));

            $(this).parent().find('.star').each(function () {
                const starValue = Number($(this).data('value'));
                $(this).toggleClass('hover', starValue <= hoverValue);
            });
        });

        interactiveRating.on('mouseleave', function () {
            $(this).find('.star').removeClass('hover');
        });
    });

    function updateCartBadge() {
        if (!config.cartCountUrl) {
            return;
        }

        $.ajax({
            url: config.cartCountUrl,
            type: 'GET',
            success: function (response) {
                if (response && typeof response.count !== 'undefined') {
                    $('#cartBadge').text(response.count);
                }
            }
        });
    }

    function saveReview() {
        if (!config.rateBookUrl) {
            console.warn('Rate book endpoint is not configured.');
            return;
        }

        const container = interactiveRating;
        const bookId = container.data('book-id');
        const ratingValue = Number(container.data('user-rating')) || 0;
        const rating = ratingValue ? ratingValue : null;
        const reviewTextRaw = reviewInput.val();
        const reviewText = reviewTextRaw ? reviewTextRaw.trim() : '';
        const trimmedReview = reviewText.length ? reviewText : null;

        if (trimmedReview && trimmedReview.length > 250) {
            showMessage('Review text must not exceed 250 characters.', 'error');
            return;
        }

        $.ajax({
            url: config.rateBookUrl,
            type: 'POST',
            data: {
                bookId: bookId,
                rating: rating,
                reviewText: trimmedReview
            },
            success: function (response) {
                if (response && response.success) {
                    if (response.averageRating) {
                        $('.star-rating-display').data('rating', response.averageRating);
                        updateStarDisplay('.star-rating-display');
                        $('.average-rating-text').text(Math.round(response.averageRating * 10) / 10);
                    } else {
                        $('.star-rating-display').data('rating', 0);
                        updateStarDisplay('.star-rating-display');
                        $('.average-rating-text').text('No ratings yet');
                    }

                    container.data('user-rating', ratingValue || 0);
                    updateStarDisplay(container);

                    if (trimmedReview) {
                        reviewInput.val(trimmedReview);
                        $('#charCount').text(trimmedReview.length);
                    }

                    showMessage(response.message, 'success');
                } else if (response && response.message) {
                    showMessage(response.message, 'error');
                } else {
                    showMessage('Error saving review. Please try again.', 'error');
                }
            },
            error: function () {
                showMessage('Error saving review. Please try again.', 'error');
            }
        });
    }

    function showMessage(message, type) {
        $('.review-message').remove();

        const messageClass = type === 'success' ? 'review-message-success' : 'review-message-error';
        const markup = '<div class=\"review-message ' + messageClass + '\">' + message + '</div>';

        $('.btn-save-review').after(markup);

        setTimeout(function () {
            $('.review-message').fadeOut(500, function () {
                $(this).remove();
            });
        }, 3000);
    }

    function updateStarDisplay(target) {
        const container = target instanceof $ ? target : $(target);
        if (!container.length) {
            return;
        }

        const ratingData = container.data('rating');
        const userRatingData = container.data('user-rating');
        const rating = typeof ratingData === 'number' ? ratingData : (typeof userRatingData === 'number' ? userRatingData : 0);

        container.find('.star').each(function () {
            const starValue = Number($(this).data('value'));
            if (starValue <= Math.round(rating || 0)) {
                $(this).addClass('filled');
            } else {
                $(this).removeClass('filled');
            }
        });
    }

    function scrollCarousel(direction) {
        const container = document.getElementById('authorBooksContainer');
        if (!container) {
            return;
        }

        const scrollAmount = 250;
        container.scrollLeft += direction === 'left' ? -scrollAmount : scrollAmount;
    }

    function toggleLikeReview(reviewId) {
        if (!config.likeReviewUrl) {
            alert('Error liking review. Please try again.');
            return;
        }

        $.ajax({
            url: config.likeReviewUrl,
            type: 'POST',
            data: { reviewId: reviewId },
            success: function (response) {
                if (response && response.success) {
                    const button = $('button[data-review-id=\"' + reviewId + '\"]');
                    button.find('.like-count').text(response.likeCount);
                    button.toggleClass('liked', !!response.isLiked);
                } else if (response && response.message) {
                    alert(response.message);
                } else {
                    alert('Error liking review. Please try again.');
                }
            },
            error: function () {
                alert('Error liking review. Please try again.');
            }
        });
    }

    function extractAndApplyGradient() {
        const bookCover = $('.book-detail-cover');

        if (!bookCover.length) {
            applyDefaultGradient();
            return;
        }

        const img = bookCover[0];

        if (img.complete && img.naturalWidth !== 0) {
            extractColors(img);
        } else {
            $(img)
                .on('load', function () {
                    extractColors(img);
                })
                .on('error', function () {
                    applyDefaultGradient();
                });
        }
    }

    function extractColors(img) {
        try {
            const colorThief = new ColorThief();
            const palette = colorThief.getPalette(img, 5);

            const colors = palette.map(function (color, index) {
                return 'rgba(' + color[0] + ', ' + color[1] + ', ' + color[2] + ', ' + (0.3 - index * 0.05) + ')';
            });

            const gradient = 'linear-gradient(135deg, ' +
                colors[0] + ' 0%, ' +
                colors[1] + ' 25%, ' +
                colors[2] + ' 50%, ' +
                colors[3] + ' 75%, ' +
                colors[4] + ' 100%)';

            page.css({
                background: gradient,
                'background-color': '#F2E4D4'
            });
        } catch (error) {
            console.error('Error extracting colors:', error);
            applyDefaultGradient();
        }
    }

    function applyDefaultGradient() {
        page.css({
            background: 'linear-gradient(135deg, rgba(212, 194, 176, 0.3) 0%, rgba(166, 109, 63, 0.2) 100%)',
            'background-color': '#F2E4D4'
        });
    }

    function initializeWishlistButton() {
        const wishlistButton = $('#wishlistButton');
        if (!wishlistButton.length || !config.wishlistStatusUrl || !config.toggleWishlistUrl) {
            return;
        }

        const bookId = wishlistButton.data('book-id');

        $.ajax({
            url: config.wishlistStatusUrl,
            type: 'GET',
            data: { bookId: bookId },
            success: function (response) {
                if (response && response.isInWishlist) {
                    updateWishlistButton(true);
                }
            }
        });

        wishlistButton.on('click', function () {
            if (wishlistButton.hasClass('loading')) {
                return;
            }

            wishlistButton.addClass('loading').prop('disabled', true);

            $.ajax({
                url: config.toggleWishlistUrl,
                type: 'POST',
                data: { bookId: bookId },
                success: function (response) {
                    if (response && response.success) {
                        updateWishlistButton(response.isInWishlist);
                        showMessage(response.message, 'success');
                    } else if (response && response.message) {
                        showMessage(response.message, 'error');
                    } else {
                        showMessage('Unable to update wishlist right now.', 'error');
                    }
                },
                error: function () {
                    showMessage('Unable to update wishlist right now.', 'error');
                },
                complete: function () {
                    wishlistButton.removeClass('loading').prop('disabled', false);
                }
            });
        });
    }

    function updateWishlistButton(isInWishlist) {
        const wishlistButton = $('#wishlistButton');
        const icon = wishlistButton.find('i');

        if (isInWishlist) {
            icon.removeClass('far').addClass('fas');
            wishlistButton.addClass('wishlist-active');
            wishlistButton.find('span').text('Remove from Wishlist');
        } else {
            icon.removeClass('fas').addClass('far');
            wishlistButton.removeClass('wishlist-active');
            wishlistButton.find('span').text('Add to Wishlist');
        }
    }

    window.saveReview = saveReview;
    window.scrollCarousel = scrollCarousel;
    window.toggleLikeReview = toggleLikeReview;
})(jQuery);
