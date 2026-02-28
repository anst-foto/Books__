// Базовый URL API (замените на адрес вашего сервера)
const API_BASE_URL = 'http://localhost:5000'; // пример, укажите свой

// DOM элементы
const form = document.getElementById('addBookForm');
const titleInput = document.getElementById('title');
const booksContainer = document.getElementById('booksContainer');
const formError = document.getElementById('formError');
const listError = document.getElementById('listError');

// Загрузить список книг при загрузке страницы
document.addEventListener('DOMContentLoaded', loadBooks);

// Обработка отправки формы
form.addEventListener('submit', async (e) => {
  e.preventDefault();
  clearError(formError);

  const title = titleInput.value.trim();
  if (!title) {
    showError(formError, 'Название книги не может быть пустым');
    return;
  }

  try {
    await createBook({ title });
    titleInput.value = '';        // очистить поле
    await loadBooks();            // обновить список
  } catch (error) {
    showError(formError, error.message);
  }
});

/**
 * Загрузить все книги с сервера и отобразить
 */
async function loadBooks() {
  clearError(listError);
  booksContainer.innerHTML = '<p class="loading">Загрузка...</p>';

  try {
    const books = await fetchBooks();
    displayBooks(books);
  } catch (error) {
    booksContainer.innerHTML = '';
    showError(listError, error.message);
  }
}

/**
 * Выполнить GET /api/v1/books
 * @returns {Promise<Array>} массив книг
 */
async function fetchBooks() {
  const url = `${API_BASE_URL}/api/v1/books`;
  const response = await fetch(url);

  if (response.status === 204) {
    return []; // нет содержимого
  }

  if (!response.ok) {
    const errorText = await response.text().catch(() => '');
    throw new Error(`Ошибка загрузки книг (${response.status}): ${errorText}`);
  }

  return await response.json();
}

/**
 * Выполнить POST /api/v1/books
 * @param {Object} bookData - объект с полем title
 * @returns {Promise<void>}
 */
async function createBook(bookData) {
  const url = `${API_BASE_URL}/api/v1/books`;
  const response = await fetch(url, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(bookData),
  });

  if (!response.ok) {
    const errorText = await response.text().catch(() => '');
    throw new Error(`Ошибка создания книги (${response.status}): ${errorText}`);
  }
  // 201 Created – успех, тело ответа обычно пустое
  return;
}

/**
 * Отобразить список книг в DOM
 * @param {Array} books
 */
function displayBooks(books) {
  if (!books || books.length === 0) {
    booksContainer.innerHTML = '<p class="empty-message">Книг пока нет. Добавьте первую!</p>';
    return;
  }

  const html = books.map(book => `
        <div class="book-card">
            <div class="book-id">ID: ${book.id || 'нет'}</div>
            <div class="book-title">${escapeHtml(book.title) || 'Без названия'}</div>
        </div>
    `).join('');

  booksContainer.innerHTML = html;
}

/**
 * Простая защита от XSS
 */
function escapeHtml(text) {
  if (!text) return '';
  const div = document.createElement('div');
  div.textContent = text;
  return div.innerHTML;
}

/**
 * Показать сообщение об ошибке в указанном элементе
 */
function showError(element, message) {
  element.textContent = message;
  element.style.display = 'block';
}

/**
 * Скрыть/очистить ошибку
 */
function clearError(element) {
  element.textContent = '';
  element.style.display = 'none';
}
