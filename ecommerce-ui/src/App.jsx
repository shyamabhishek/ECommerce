import { useState, useEffect } from "react";
import "./App.css";

function App() {
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  // 1. Base URL for your .NET API
  const API_BASE_URL = "https://localhost:7129";

  useEffect(() => {
    fetch(`${API_BASE_URL}/api/Product`)
      .then((response) => {
        if (!response.ok) throw new Error(`API Error: ${response.status}`);
        return response.json();
      })
      .then((data) => {
        // Log to verify: You should now see 'id' (not 0) and 'pictureUrl' (not null)
        console.log("API Data:", data);

        // If your API returns a PagedList object, use data.items.
        // If it returns the array directly, use data.
        setProducts(data);
        setLoading(false);
      })
      .catch((err) => {
        setError(err.message);
        setLoading(false);
      });
  }, []);

  if (loading) return <div className="loader">Loading Store...</div>;
  if (error) return <div className="error">Error: {error}</div>;

  return (
    <div className="container">
      <header>
        <h1>My E-Commerce Store 🛒</h1>
      </header>

      <main className="product-grid">
        {products.map((product) => (
          /* 2. Uses the real Database ID for the key */
          <div key={product.id} className="product-card">
            <div className="image-container">
              {product.pictureUrl ? (
                <img
                  /* 3. Uses the real path from the DB */
                  src={`${API_BASE_URL}/${product.pictureUrl.replace(/\\/g, "/")}`}
                  alt={product.name}
                  onError={(e) => {
                    e.target.src =
                      "https://placehold.co/300x200?text=Image+Not+Found";
                  }}
                />
              ) : (
                <img
                  src="https://placehold.co/300x200?text=No+Image"
                  alt="Placeholder"
                />
              )}
            </div>

            <div className="product-info">
              <h3>{product.name}</h3>
              <p className="description">{product.description}</p>
              <p className="price">
                $
                {product.price?.toLocaleString(undefined, {
                  minimumFractionDigits: 2,
                })}
              </p>
              <button className="add-button">Add to Cart</button>
            </div>
          </div>
        ))}
      </main>
    </div>
  );
}

export default App;
