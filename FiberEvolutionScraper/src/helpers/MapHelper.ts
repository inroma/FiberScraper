export class MapHelper {

    static getTileLayers = () => [
        {
            name: 'OpenStreetMap',
            visible: true,
            attribution:
            '&copy; <a target="_blank" href="http://osm.org/copyright">OpenStreetMap</a> contributors',
            url: 'https://tile.openstreetmap.org/{z}/{x}/{y}.png'
        },
        {
            name: 'CartoDBDark',
            visible: false,
            url: 'https://basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png',
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors &copy; <a href="https://carto.com/attributions">CARTO</a>',
        },
        {
            name: 'OpenTopoMap',
            visible: false,
            url: 'https://tile.opentopomap.org/{z}/{x}/{y}.png',
            attribution:
            'Map data: &copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>, <a href="http://viewfinderpanoramas.org">SRTM</a> | Map style: &copy; <a href="https://opentopomap.org">OpenTopoMap</a> (<a href="https://creativecommons.org/licenses/by-sa/3.0/">CC-BY-SA</a>)',
        }
    ];

    static maxBounds = [ -6, 41, 9.5, 52 ];
}