(function () {
    const form = document.getElementById('referenceLookupForm');
    if (!form) return;

    const message = document.getElementById('lookupMessage');
    const resultCard = document.getElementById('lookupResult');

    const referenceEl = document.getElementById('resultReference');
    const statusEl = document.getElementById('resultStatus');
    const titleEl = document.getElementById('resultTitle');
    const authorEl = document.getElementById('resultAuthor');
    const userEl = document.getElementById('resultUser');
    const orgEl = document.getElementById('resultOrganization');
    const createdEl = document.getElementById('timelineCreated');
    const expiresEl = document.getElementById('timelineExpires');
    const claimedEl = document.getElementById('timelineClaimed');
    const claimedWrapper = document.getElementById('timelineClaimedWrapper');
    const checkedOutEl = document.getElementById('timelineCheckedOut');
    const checkedOutWrapper = document.getElementById('timelineCheckedOutWrapper');
    const dueEl = document.getElementById('timelineDue');
    const dueWrapper = document.getElementById('timelineDueWrapper');
    const returnedEl = document.getElementById('timelineReturned');
    const returnedWrapper = document.getElementById('timelineReturnedWrapper');
    const notesContainer = document.getElementById('resultNotesContainer');
    const notesEl = document.getElementById('resultNotes');

    function resetTimeline() {
        [claimedWrapper, checkedOutWrapper, dueWrapper, returnedWrapper].forEach(function (item) {
            if (item) item.style.display = 'none';
        });
    }

    function showMessage(text, isSuccess) {
        if (!message) return;
        message.textContent = text || '';
        message.classList.remove('success', 'error', 'show');
        if (text) {
            message.classList.add(isSuccess ? 'success' : 'error');
            message.classList.add('show');
        }
    }

    function toggleResultCard(show) {
        if (!resultCard) return;
        if (show) resultCard.classList.add('show'); else resultCard.classList.remove('show');
    }

    function updateStatusBadge(statusText) {
        if (!statusEl) return;
        statusEl.textContent = statusText || 'Unknown';
        statusEl.className = 'result-status';
        if (!statusText) {
            statusEl.classList.add('status-unknown');
            return;
        }
        const normalized = statusText.toLowerCase().replace(/\s+/g, '-');
        statusEl.classList.add('status-' + normalized);
    }

    function updateResult(ticket) {
        if (referenceEl) referenceEl.textContent = ticket.referenceCode || '-';
        if (titleEl) titleEl.textContent = ticket.bookTitle || 'Untitled';
        if (authorEl) authorEl.textContent = ticket.bookAuthor || '';
        if (userEl) userEl.textContent = ticket.userName || 'Unknown borrower';
        if (orgEl) orgEl.textContent = ticket.organizationName || '';
        if (createdEl) createdEl.textContent = ticket.createdAt || '—';
        if (expiresEl) expiresEl.textContent = ticket.expiresAt || '—';

        resetTimeline();

        if (ticket.claimedAt && claimedEl && claimedWrapper) {
            claimedEl.textContent = ticket.claimedAt;
            claimedWrapper.style.display = 'flex';
        }
        if (ticket.checkedOutAt && checkedOutEl && checkedOutWrapper) {
            checkedOutEl.textContent = ticket.checkedOutAt;
            checkedOutWrapper.style.display = 'flex';
        }
        if (ticket.dueAt && dueEl && dueWrapper) {
            dueEl.textContent = ticket.dueAt;
            dueWrapper.style.display = 'flex';
        }
        if (ticket.returnedAt && returnedEl && returnedWrapper) {
            returnedEl.textContent = ticket.returnedAt;
            returnedWrapper.style.display = 'flex';
        }

        if (notesContainer && notesEl) {
            if (ticket.notes) {
                notesEl.textContent = ticket.notes;
                notesContainer.style.display = 'block';
            } else {
                notesContainer.style.display = 'none';
            }
        }

        updateStatusBadge(ticket.status);
        toggleResultCard(true);
    }

    function clearResult() {
        toggleResultCard(false);
        resetTimeline();
    }

    function fetchJson(url, params) {
        return fetch(url, params).then(function (r) { return r.json(); });
    }

    function buildReferenceCode(org, datePart, numberPart) {
        return org + '-' + datePart + '-' + numberPart;
    }

    const organizationCode = (function () {
        const prefix = document.querySelector('.reference-prefix');
        if (!prefix) return 'ORG';
        const txt = prefix.textContent || '';
        const dash = txt.indexOf('-');
        return dash > 0 ? txt.substring(0, dash) : (txt || 'ORG');
    })();

    const urls = {
        check: '/AdminHome/CheckReference',
        pending: '/AdminHome/GetPendingClaims',
        unreturned: '/AdminHome/GetUnreturnedBooks',
        confirmClaim: '/AdminHome/ConfirmClaim',
        confirmReturn: '/AdminHome/ConfirmReturn'
    };

    form.addEventListener('submit', function (event) {
        event.preventDefault();
        const dateInput = (document.getElementById('dateInput') || {}).value || '';
        const numberInput = (document.getElementById('numberInput') || {}).value || '';

        const datePart = dateInput.trim();
        const numberPart = numberInput.trim();

        if (!datePart || !numberPart) {
            showMessage('Please enter both date and ticket number.', false);
            clearResult();
            return;
        }
        if (!/^\d{8}$/.test(datePart)) {
            showMessage('Please enter a valid date in YYYYMMDD format.', false);
            clearResult();
            return;
        }
        if (!/^\d{5}$/.test(numberPart)) {
            showMessage('Please enter a valid 5-digit ticket number.', false);
            clearResult();
            return;
        }

        const referenceCode = buildReferenceCode(organizationCode, datePart, numberPart);
        showMessage('Searching...', true);

        fetchJson(urls.check, {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
            body: new URLSearchParams({ referenceCode: referenceCode }).toString()
        }).then(function (result) {
            if (result && result.success) {
                showMessage('Reference found. Review the reservation details below.', true);
                updateResult(result.ticket);
            } else {
                const errorMessage = (result && result.message) ? result.message : 'Reference number not found.';
                showMessage(errorMessage, false);
                clearResult();
            }
        }).catch(function () {
            showMessage('Unable to verify the reference number right now.', false);
            clearResult();
        });
    });

    window.loadPendingClaims = function () {
        fetchJson(urls.pending).then(function (data) {
            const container = document.getElementById('pendingClaimsList');
            if (!container) return;
            if (data && data.success && data.claims && data.claims.length > 0) {
                container.innerHTML = '';
                data.claims.forEach(function (claim) { container.appendChild(createPendingClaimCard(claim)); });
            } else {
                container.innerHTML = '<p class="no-pending-claims">No pending claims at this time.</p>';
            }
        }).catch(function () {
            const container = document.getElementById('pendingClaimsList');
            if (container) container.innerHTML = '<p class="no-pending-claims">Unable to load pending claims.</p>';
        });
    };

    window.createPendingClaimCard = function (claim) {
        const card = document.createElement('div');
        card.className = 'pending-claim-card';
        card.innerHTML = (
            '<div class="claim-info">'
            + '<div class="claim-header">'
            + '<h4>' + claim.bookTitle + '</h4>'
            + '<span class="claim-reference">' + claim.referenceCode + '</span>'
            + '</div>'
            + '<div class="claim-details">'
            + '<p><strong>Borrower:</strong> ' + claim.userName + '</p>'
            + '<p><strong>Claim by:</strong> ' + claim.expiresAt + '</p>'
            + '</div>'
            + '</div>'
            + '<div class="claim-actions">'
            + '<button class="btn-confirm-claim" onclick="confirmClaim(\'' + claim.referenceCode + '\')">'
            + '<i class="fas fa-check"></i> Confirm Claim'
            + '</button>'
            + '</div>'
        );
        card.onclick = function (e) {
            if (!e.target.closest('.btn-confirm-claim')) {
                const parts = claim.referenceCode.split('-');
                if (parts.length === 3) {
                    document.getElementById('dateInput').value = parts[1];
                    document.getElementById('numberInput').value = parts[2];
                    document.getElementById('referenceLookupForm').dispatchEvent(new Event('submit'));
                }
            }
        };
        return card;
    };

    window.confirmClaim = function (referenceCode) {
        appModal.confirm({
            title: 'Confirm Claim',
            message: 'Confirm this claim? The book will be marked as borrowed.',
            okText: 'Confirm',
            cancelText: 'Cancel',
            variant: 'confirm'
        }).then(function (ok) {
            if (!ok) return;
            fetchJson(urls.confirmClaim, {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
                body: new URLSearchParams({ referenceCode: referenceCode, notes: 'Confirmed by admin' }).toString()
            }).then(function (result) {
                if (result && result.success) {
                    appModal.alert({ title: 'Success', message: 'Claim confirmed successfully!', variant: 'success', okText: 'OK' });
                    loadPendingClaims();
                } else {
                    appModal.alert({ title: 'Error', message: (result && result.message) || 'Failed to confirm claim.', variant: 'error' });
                }
            }).catch(function () {
                appModal.alert({ title: 'Error', message: 'Unable to confirm claim right now.', variant: 'error' });
            });
        });
    };

    window.loadUnreturnedBooks = function () {
        fetchJson(urls.unreturned).then(function (data) {
            const container = document.getElementById('unreturnedBooksList');
            if (!container) return;
            if (data && data.success && data.books && data.books.length > 0) {
                container.innerHTML = '';
                data.books.forEach(function (book) { container.appendChild(createUnreturnedBookCard(book)); });
            } else {
                container.innerHTML = '<p class="no-unreturned-books">No unreturned books at this time.</p>';
            }
        }).catch(function () {
            const container = document.getElementById('unreturnedBooksList');
            if (container) container.innerHTML = '<p class="no-unreturned-books">Unable to load unreturned books.</p>';
        });
    };

    window.createUnreturnedBookCard = function (book) {
        const card = document.createElement('div');
        card.className = 'unreturned-book-card' + (book.isOverdue ? ' overdue' : '');
        card.innerHTML = (
            '<div class="book-info">'
            + '<div class="book-header">'
            + '<h4>' + book.bookTitle + '</h4>'
            + '<span class="book-reference">' + book.referenceCode + '</span>'
            + '</div>'
            + '<div class="book-details">'
            + '<p><strong>Borrower:</strong> ' + book.userName + '</p>'
            + '<p><strong>Checked Out:</strong> ' + book.checkedOutAt + '</p>'
            + '<p><strong>Due:</strong> ' + book.dueAt + '</p>'
            + (book.isOverdue ? '<p class="overdue-indicator"><i class="fas fa-exclamation-triangle"></i> <strong>OVERDUE</strong> (' + book.daysOverdue + ' days)</p>' : '')
            + '</div>'
            + '</div>'
            + '<div class="book-actions">'
            + '<button class="btn-confirm-return" onclick="confirmReturn(\'' + book.referenceCode + '\')">'
            + '<i class="fas fa-check"></i> Confirm Return'
            + '</button>'
            + '</div>'
        );
        card.onclick = function (e) {
            if (!e.target.closest('.btn-confirm-return')) {
                const parts = book.referenceCode.split('-');
                if (parts.length === 3) {
                    document.getElementById('dateInput').value = parts[1];
                    document.getElementById('numberInput').value = parts[2];
                    document.getElementById('referenceLookupForm').dispatchEvent(new Event('submit'));
                }
            }
        };
        return card;
    };

    window.confirmReturn = function (referenceCode) {
        appModal.confirm({
            title: 'Confirm Return',
            message: 'Confirm this book return?',
            okText: 'Confirm',
            cancelText: 'Cancel',
            variant: 'confirm'
        }).then(function (ok) {
            if (!ok) return;
            fetchJson(urls.confirmReturn, {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
                body: new URLSearchParams({ referenceCode: referenceCode }).toString()
            }).then(function (result) {
                if (result && result.success) {
                    appModal.alert({ title: 'Success', message: 'Book return confirmed successfully!', variant: 'success' });
                    loadUnreturnedBooks();
                } else {
                    appModal.alert({ title: 'Error', message: (result && result.message) || 'Failed to confirm return.', variant: 'error' });
                }
            }).catch(function () {
                appModal.alert({ title: 'Error', message: 'Unable to confirm return right now.', variant: 'error' });
            });
        });
    };

    // Initial fetches
    try { loadPendingClaims(); } catch (e) {}
    try { loadUnreturnedBooks(); } catch (e) {}
})();
